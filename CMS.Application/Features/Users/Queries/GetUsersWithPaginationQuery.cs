using CMS.Application.Common.Interfaces;
using CMS.Application.Common.Models;
using CMS.Application.Features.Users.DTOs;
using CMS.Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CMS.Application.Features.Users.Queries;

/// <summary>
/// Query to get users with pagination, filtering, and sorting.
/// </summary>
public sealed record GetUsersWithPaginationQuery : IRequest<PaginatedList<UserListDto>>
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public string? SearchTerm { get; init; }
    public UserRole? RoleFilter { get; init; }
    public bool? IsActiveFilter { get; init; }
    public string? SortBy { get; init; }
    public bool SortDescending { get; init; }
}

/// <summary>
/// Validator for GetUsersWithPaginationQuery.
/// </summary>
public sealed class GetUsersWithPaginationQueryValidator : AbstractValidator<GetUsersWithPaginationQuery>
{
    public GetUsersWithPaginationQueryValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1).WithMessage("Page number must be at least 1.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage("Page size must be between 1 and 100.");
    }
}

/// <summary>
/// Handler for GetUsersWithPaginationQuery.
/// </summary>
public sealed class GetUsersWithPaginationQueryHandler
    : IRequestHandler<GetUsersWithPaginationQuery, PaginatedList<UserListDto>>
{
    private readonly IApplicationDbContext _context;

    public GetUsersWithPaginationQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedList<UserListDto>> Handle(
        GetUsersWithPaginationQuery request,
        CancellationToken cancellationToken)
    {
        var query = _context.Users.AsNoTracking();

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchTerm = request.SearchTerm.ToLower().Trim();
            query = query.Where(u =>
                u.Email.ToLower().Contains(searchTerm) ||
                u.FirstName.ToLower().Contains(searchTerm) ||
                u.LastName.ToLower().Contains(searchTerm));
        }

        // Apply role filter
        if (request.RoleFilter.HasValue)
        {
            query = query.Where(u => u.Role == request.RoleFilter.Value);
        }

        // Apply active status filter
        if (request.IsActiveFilter.HasValue)
        {
            query = query.Where(u => u.IsActive == request.IsActiveFilter.Value);
        }

        // Apply sorting
        query = ApplySorting(query, request.SortBy, request.SortDescending);

        // Project to DTO and paginate
        var projectedQuery = query.Select(u => new UserListDto
        {
            Id = u.Id,
            Email = u.Email,
            FirstName = u.FirstName,
            LastName = u.LastName,
            FullName = u.FirstName + " " + u.LastName,
            Role = u.Role,
            IsActive = u.IsActive,
            LastLoginAt = u.LastLoginAt,
            CreatedAt = u.CreatedAt
        });

        return await PaginatedList<UserListDto>.CreateAsync(
            projectedQuery,
            request.PageNumber,
            request.PageSize,
            cancellationToken);
    }

    private static IQueryable<Domain.Entities.User> ApplySorting(
        IQueryable<Domain.Entities.User> query,
        string? sortBy,
        bool sortDescending)
    {
        return sortBy?.ToLowerInvariant() switch
        {
            "email" => sortDescending
                ? query.OrderByDescending(u => u.Email)
                : query.OrderBy(u => u.Email),
            "firstname" => sortDescending
                ? query.OrderByDescending(u => u.FirstName)
                : query.OrderBy(u => u.FirstName),
            "lastname" => sortDescending
                ? query.OrderByDescending(u => u.LastName)
                : query.OrderBy(u => u.LastName),
            "role" => sortDescending
                ? query.OrderByDescending(u => u.Role)
                : query.OrderBy(u => u.Role),
            "lastloginat" => sortDescending
                ? query.OrderByDescending(u => u.LastLoginAt)
                : query.OrderBy(u => u.LastLoginAt),
            "createdat" => sortDescending
                ? query.OrderByDescending(u => u.CreatedAt)
                : query.OrderBy(u => u.CreatedAt),
            _ => query.OrderByDescending(u => u.CreatedAt)
        };
    }
}