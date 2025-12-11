using CMS.Application.Common.Extensions;
using CMS.Application.Common.Interfaces;
using CMS.Application.Common.Models.Search;
using CMS.Application.Common.Services;
using CMS.Application.Common.Validators;
using CMS.Application.Features.Media.DTOs;
using CMS.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CMS.Application.Features.Media.Queries;

/// <summary>
/// Advanced search query for images with full-text search, filtering, sorting, and paging
/// </summary>
public sealed record SearchImagesQuery : SearchRequest, IRequest<SearchResult<ImageListDto>>
{
}

/// <summary>
/// Validator for SearchImagesQuery
/// </summary>
public sealed class SearchImagesQueryValidator : AbstractValidator<SearchImagesQuery>
{
    public SearchImagesQueryValidator()
    {
        Include(new SearchRequestValidator());

        When(x => x.Filters != null, () =>
        {
            RuleFor(x => x.Filters)
                .Must(filters => filters!.All(f => IsAllowedField(f.Field)))
                .WithMessage("One or more filter fields are not allowed for Image search");
        });

        When(x => x.Sorts != null, () =>
        {
            RuleFor(x => x.Sorts)
                .Must(sorts => sorts!.All(s => IsAllowedSortField(s.Field)))
                .WithMessage("One or more sort fields are not allowed for Image search");
        });
    }

    private static bool IsAllowedField(string field)
    {
        var allowedFields = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            nameof(ImageEntity.FileName),
            nameof(ImageEntity.OriginalName),
            nameof(ImageEntity.ContentType),
            nameof(ImageEntity.Size),
            nameof(ImageEntity.Width),
            nameof(ImageEntity.Height),
            nameof(ImageEntity.AltText),
            nameof(ImageEntity.Caption),
            nameof(ImageEntity.FolderId),
            nameof(ImageEntity.UploadedByUserId),
            nameof(ImageEntity.CreatedAt),
            nameof(ImageEntity.LastModifiedAt)
        };

        return allowedFields.Contains(field);
    }

    private static bool IsAllowedSortField(string field)
    {
        var allowedFields = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            nameof(ImageEntity.FileName),
            nameof(ImageEntity.OriginalName),
            nameof(ImageEntity.ContentType),
            nameof(ImageEntity.Size),
            nameof(ImageEntity.Width),
            nameof(ImageEntity.Height),
            nameof(ImageEntity.CreatedAt),
            nameof(ImageEntity.LastModifiedAt)
        };

        return allowedFields.Contains(field);
    }
}

/// <summary>
/// Handler for SearchImagesQuery
/// </summary>
public sealed class SearchImagesQueryHandler : IRequestHandler<SearchImagesQuery, SearchResult<ImageListDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ISearchService _searchService;

    public SearchImagesQueryHandler(IApplicationDbContext context, ISearchService searchService)
    {
        _context = context;
        _searchService = searchService;
    }

    public async Task<SearchResult<ImageListDto>> Handle(
        SearchImagesQuery request,
        CancellationToken cancellationToken)
    {
        // Configure search for ImageEntity
        var searchConfig = new SearchConfiguration<ImageEntity>()
            .AddSearchableField(i => i.OriginalName, 'A')        // Highest weight
            .AddSearchableField(i => i.FileName, 'B')            // High weight
            .AddSearchableField(i => i.AltText, 'B')             // High weight
            .AddSearchableField(i => i.Caption, 'C')             // Medium weight
            .AddFilterableField(i => i.FileName, "Equals", "Contains", "StartsWith")
            .AddFilterableField(i => i.OriginalName, "Equals", "Contains", "StartsWith")
            .AddFilterableField(i => i.ContentType, "Equals", "In")
            .AddFilterableField(i => i.Size, "Equals", "GreaterThan", "LessThan", "GreaterThanOrEqual", "LessThanOrEqual", "Between")
            .AddFilterableField(i => i.Width, "Equals", "GreaterThan", "LessThan", "GreaterThanOrEqual", "LessThanOrEqual", "Between")
            .AddFilterableField(i => i.Height, "Equals", "GreaterThan", "LessThan", "GreaterThanOrEqual", "LessThanOrEqual", "Between")
            .AddFilterableField(i => i.AltText, "Equals", "Contains", "IsNull", "IsNotNull")
            .AddFilterableField(i => i.Caption, "Equals", "Contains", "IsNull", "IsNotNull")
            .AddFilterableField(i => i.FolderId, "Equals", "In", "IsNull", "IsNotNull")
            .AddFilterableField(i => i.UploadedByUserId, "Equals", "In")
            .AddFilterableField(i => i.CreatedAt, "Equals", "GreaterThan", "LessThan", "GreaterThanOrEqual", "LessThanOrEqual", "Between")
            .AddFilterableField(i => i.LastModifiedAt, "Equals", "GreaterThan", "LessThan", "GreaterThanOrEqual", "LessThanOrEqual", "Between", "IsNull", "IsNotNull")
            .AddSortableField(i => i.FileName)
            .AddSortableField(i => i.OriginalName)
            .AddSortableField(i => i.ContentType)
            .AddSortableField(i => i.Size)
            .AddSortableField(i => i.Width)
            .AddSortableField(i => i.Height)
            .AddSortableField(i => i.CreatedAt)
            .AddSortableField(i => i.LastModifiedAt)
            .SetDefaultSort(i => i.CreatedAt, descending: true);

        // Base query with includes for related data
        var query = _context.Images
            .AsNoTracking()
            .Include(i => i.Folder)
            .Include(i => i.UploadedByUser);

        // Execute search
        var searchResult = await _searchService.SearchAsync(
            query,
            request,
            searchConfig,
            cancellationToken);

        // Project to DTO
        var dtoResult = new SearchResult<ImageListDto>
        {
            Items = searchResult.Items.Select(item => new SearchResultItem<ImageListDto>
            {
                Data = new ImageListDto
                {
                    Id = item.Data.Id,
                    FileName = item.Data.FileName,
                    OriginalName = item.Data.OriginalName,
                    ContentType = item.Data.ContentType,
                    Size = item.Data.Size,
                    Width = item.Data.Width,
                    Height = item.Data.Height,
                    AltText = item.Data.AltText,
                    Caption = item.Data.Caption,
                    HasThumbnail = item.Data.ThumbnailData != null && item.Data.ThumbnailData.Length > 0,
                    HasMediumVersion = item.Data.MediumData != null && item.Data.MediumData.Length > 0,
                    FolderId = item.Data.FolderId,
                    FolderName = item.Data.Folder?.Name,
                    UploadedByUserId = item.Data.UploadedByUserId,
                    UploadedByUserName = item.Data.UploadedByUser != null
                        ? $"{item.Data.UploadedByUser.FirstName} {item.Data.UploadedByUser.LastName}"
                        : null,
                    CreatedAt = item.Data.CreatedAt,
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
