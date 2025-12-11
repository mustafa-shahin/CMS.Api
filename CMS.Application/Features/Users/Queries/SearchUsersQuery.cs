using CMS.Application.Common.Extensions;
using CMS.Application.Common.Interfaces;
using CMS.Application.Common.Models.Search;
using CMS.Application.Common.Services;
using CMS.Application.Common.Validators;
using CMS.Application.Features.Users.DTOs;
using CMS.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CMS.Application.Features.Users.Queries;

/// <summary>
/// Advanced search query for users with full-text search, filtering, sorting, and paging
/// </summary>
public sealed record SearchUsersQuery : SearchRequest, IRequest<SearchResult<UserListDto>>
{
}

/// <summary>
/// Validator for SearchUsersQuery
/// </summary>
public sealed class SearchUsersQueryValidator : AbstractValidator<SearchUsersQuery>
{
    public SearchUsersQueryValidator()
    {
        Include(new SearchRequestValidator());

        // Additional user-specific validation can be added here
        When(x => x.Filters != null, () =>
        {
            RuleFor(x => x.Filters)
                .Must(filters => filters!.All(f => IsAllowedField(f.Field)))
                .WithMessage("One or more filter fields are not allowed for User search");
        });

        When(x => x.Sorts != null, () =>
        {
            RuleFor(x => x.Sorts)
                .Must(sorts => sorts!.All(s => IsAllowedSortField(s.Field)))
                .WithMessage("One or more sort fields are not allowed for User search");
        });
    }

    private static bool IsAllowedField(string field)
    {
        var allowedFields = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            nameof(User.Email),
            nameof(User.FirstName),
            nameof(User.LastName),
            nameof(User.Role),
            nameof(User.IsActive),
            nameof(User.LastLoginAt),
            nameof(User.CreatedAt),
            nameof(User.LastModifiedAt)
        };

        return allowedFields.Contains(field);
    }

    private static bool IsAllowedSortField(string field)
    {
        var allowedFields = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            nameof(User.Email),
            nameof(User.FirstName),
            nameof(User.LastName),
            nameof(User.Role),
            nameof(User.IsActive),
            nameof(User.LastLoginAt),
            nameof(User.CreatedAt),
            nameof(User.LastModifiedAt)
        };

        return allowedFields.Contains(field);
    }
}

/// <summary>
/// Handler for SearchUsersQuery
/// </summary>
public sealed class SearchUsersQueryHandler : IRequestHandler<SearchUsersQuery, SearchResult<UserListDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ISearchService _searchService;

    public SearchUsersQueryHandler(IApplicationDbContext context, ISearchService searchService)
    {
        _context = context;
        _searchService = searchService;
    }

    public async Task<SearchResult<UserListDto>> Handle(
        SearchUsersQuery request,
        CancellationToken cancellationToken)
    {
        // Configure search for User entity
        var searchConfig = new SearchConfiguration<User>()
            .AddSearchableField(u => u.Email, 'A')           // Highest weight
            .AddSearchableField(u => u.FirstName, 'B')       // High weight
            .AddSearchableField(u => u.LastName, 'B')        // High weight
            .AddFilterableField(u => u.Email, "Equals", "Contains", "StartsWith")
            .AddFilterableField(u => u.FirstName, "Equals", "Contains", "StartsWith")
            .AddFilterableField(u => u.LastName, "Equals", "Contains", "StartsWith")
            .AddFilterableField(u => u.Role, "Equals", "In")
            .AddFilterableField(u => u.IsActive, "Equals")
            .AddFilterableField(u => u.LastLoginAt, "Equals", "GreaterThan", "LessThan", "GreaterThanOrEqual", "LessThanOrEqual", "Between")
            .AddFilterableField(u => u.CreatedAt, "Equals", "GreaterThan", "LessThan", "GreaterThanOrEqual", "LessThanOrEqual", "Between")
            .AddFilterableField(u => u.LastModifiedAt, "Equals", "GreaterThan", "LessThan", "GreaterThanOrEqual", "LessThanOrEqual", "Between")
            .AddSortableField(u => u.Email)
            .AddSortableField(u => u.FirstName)
            .AddSortableField(u => u.LastName)
            .AddSortableField(u => u.Role)
            .AddSortableField(u => u.IsActive)
            .AddSortableField(u => u.LastLoginAt)
            .AddSortableField(u => u.CreatedAt)
            .AddSortableField(u => u.LastModifiedAt)
            .SetDefaultSort(u => u.CreatedAt, descending: true);

        // Base query
        var query = _context.Users.AsNoTracking();

        // Execute search
        var searchResult = await _searchService.SearchAsync(
            query,
            request,
            searchConfig,
            cancellationToken);

        // Project to DTO
        var dtoResult = new SearchResult<UserListDto>
        {
            Items = searchResult.Items.Select(item => new SearchResultItem<UserListDto>
            {
                Data = new UserListDto
                {
                    Id = item.Data.Id,
                    Email = item.Data.Email,
                    FirstName = item.Data.FirstName,
                    LastName = item.Data.LastName,
                    FullName = item.Data.FirstName + " " + item.Data.LastName,
                    Role = item.Data.Role,
                    IsActive = item.Data.IsActive,
                    LastLoginAt = item.Data.LastLoginAt,
                    CreatedAt = item.Data.CreatedAt
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
