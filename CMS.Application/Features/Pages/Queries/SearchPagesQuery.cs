using CMS.Application.Common.Extensions;
using CMS.Application.Common.Interfaces;
using CMS.Application.Common.Models.Search;
using CMS.Application.Common.Services;
using CMS.Application.Common.Validators;
using CMS.Application.Features.Pages.DTOs;
using CMS.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CMS.Application.Features.Pages.Queries;

/// <summary>
/// Advanced search query for pages with full-text search, filtering, sorting, and paging
/// </summary>
public sealed record SearchPagesQuery : SearchRequest, IRequest<SearchResult<PageListDto>>
{
}

/// <summary>
/// Validator for SearchPagesQuery
/// </summary>
public sealed class SearchPagesQueryValidator : AbstractValidator<SearchPagesQuery>
{
    public SearchPagesQueryValidator()
    {
        Include(new SearchRequestValidator());

        When(x => x.Filters != null, () =>
        {
            RuleFor(x => x.Filters)
                .Must(filters => filters!.All(f => IsAllowedField(f.Field)))
                .WithMessage("One or more filter fields are not allowed for Page search");
        });

        When(x => x.Sorts != null, () =>
        {
            RuleFor(x => x.Sorts)
                .Must(sorts => sorts!.All(s => IsAllowedSortField(s.Field)))
                .WithMessage("One or more sort fields are not allowed for Page search");
        });
    }

    private static bool IsAllowedField(string field)
    {
        var allowedFields = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            nameof(Page.Title),
            nameof(Page.Slug),
            nameof(Page.Status),
            nameof(Page.MetaTitle),
            nameof(Page.MetaDescription),
            nameof(Page.PublishedAt),
            nameof(Page.Version),
            nameof(Page.CreatedByUserId),
            nameof(Page.CreatedAt),
            nameof(Page.UpdatedByUserId),
            nameof(Page.LastModifiedAt)
        };

        return allowedFields.Contains(field);
    }

    private static bool IsAllowedSortField(string field)
    {
        var allowedFields = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            nameof(Page.Title),
            nameof(Page.Slug),
            nameof(Page.Status),
            nameof(Page.PublishedAt),
            nameof(Page.Version),
            nameof(Page.CreatedAt),
            nameof(Page.LastModifiedAt)
        };

        return allowedFields.Contains(field);
    }
}

/// <summary>
/// Handler for SearchPagesQuery
/// </summary>
public sealed class SearchPagesQueryHandler : IRequestHandler<SearchPagesQuery, SearchResult<PageListDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ISearchService _searchService;

    public SearchPagesQueryHandler(IApplicationDbContext context, ISearchService searchService)
    {
        _context = context;
        _searchService = searchService;
    }

    public async Task<SearchResult<PageListDto>> Handle(
        SearchPagesQuery request,
        CancellationToken cancellationToken)
    {
        // Configure search for Page entity
        var searchConfig = new SearchConfiguration<Page>()
            .AddSearchableField(p => p.Title, 'A')               // Highest weight
            .AddSearchableField(p => p.Slug, 'B')                // High weight
            .AddSearchableField(p => p.MetaTitle, 'C')           // Medium weight
            .AddSearchableField(p => p.MetaDescription, 'D')     // Low weight
            .AddFilterableField(p => p.Title, "Equals", "Contains", "StartsWith")
            .AddFilterableField(p => p.Slug, "Equals", "Contains", "StartsWith")
            .AddFilterableField(p => p.Status, "Equals", "In")
            .AddFilterableField(p => p.MetaTitle, "Equals", "Contains")
            .AddFilterableField(p => p.MetaDescription, "Equals", "Contains")
            .AddFilterableField(p => p.PublishedAt, "Equals", "GreaterThan", "LessThan", "GreaterThanOrEqual", "LessThanOrEqual", "Between", "IsNull", "IsNotNull")
            .AddFilterableField(p => p.Version, "Equals", "GreaterThan", "LessThan", "GreaterThanOrEqual", "LessThanOrEqual")
            .AddFilterableField(p => p.CreatedByUserId, "Equals", "In")
            .AddFilterableField(p => p.CreatedAt, "Equals", "GreaterThan", "LessThan", "GreaterThanOrEqual", "LessThanOrEqual", "Between")
            .AddFilterableField(p => p.UpdatedByUserId, "Equals", "In")
            .AddFilterableField(p => p.LastModifiedAt, "Equals", "GreaterThan", "LessThan", "GreaterThanOrEqual", "LessThanOrEqual", "Between")
            .AddSortableField(p => p.Title)
            .AddSortableField(p => p.Slug)
            .AddSortableField(p => p.Status)
            .AddSortableField(p => p.PublishedAt)
            .AddSortableField(p => p.Version)
            .AddSortableField(p => p.CreatedAt)
            .AddSortableField(p => p.LastModifiedAt)
            .SetDefaultSort(p => p.LastModifiedAt, descending: true);

        // Base query with includes for user names
        var query = _context.Pages
            .AsNoTracking()
            .Include(p => p.CreatedByUser)
            .Include(p => p.UpdatedByUser);

        // Execute search
        var searchResult = await _searchService.SearchAsync(
            query,
            request,
            searchConfig,
            cancellationToken);

        // Project to DTO
        var dtoResult = new SearchResult<PageListDto>
        {
            Items = searchResult.Items.Select(item => new SearchResultItem<PageListDto>
            {
                Data = new PageListDto
                {
                    Id = item.Data.Id,
                    Title = item.Data.Title,
                    Slug = item.Data.Slug,
                    Status = item.Data.Status,
                    MetaTitle = item.Data.MetaTitle,
                    MetaDescription = item.Data.MetaDescription,
                    PublishedAt = item.Data.PublishedAt,
                    Version = item.Data.Version,
                    CreatedByUserId = item.Data.CreatedByUserId,
                    CreatedByUserName = item.Data.CreatedByUser != null
                        ? $"{item.Data.CreatedByUser.FirstName} {item.Data.CreatedByUser.LastName}"
                        : null,
                    CreatedAt = item.Data.CreatedAt,
                    LastModifiedByUserId = item.Data.UpdatedByUserId,
                    LastModifiedByUserName = item.Data.UpdatedByUser != null
                        ? $"{item.Data.UpdatedByUser.FirstName} {item.Data.UpdatedByUser.LastName}"
                        : null,
                    LastModifiedAt = item.Data.LastModifiedAt
                },
                RelevanceScore = item.RelevanceScore,
                Highlights = item.Highlights
            }).ToList(),
            TotalCount = searchResult.TotalCount,
            PageNumber = searchResult.PageNumber,
            PageSize = searchResult.PageSize,
            ExecutionTimeMs = searchResult.ExecutionTimeMs,
            AppliedFilters = searchResult.AppliedFilters,
            AppliedSorts = searchResult.AppliedSorts,
            SearchTerm = searchResult.SearchTerm,
            Facets = searchResult.Facets
        };

        return dtoResult;
    }
}
