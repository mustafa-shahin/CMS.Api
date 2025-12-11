using CMS.Application.Common.Interfaces;
using CMS.Application.Common.Models;
using CMS.Application.Features.Media.DTOs;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CMS.Application.Features.Media.Queries.GetImagesWithPagination;

/// <summary>
/// Query to get images with pagination, filtering, and sorting.
/// </summary>
public sealed record GetImagesWithPaginationQuery : IRequest<PaginatedList<ImageListDto>>
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 12;
    public string? SearchTerm { get; init; }
    public int? FolderId { get; init; }
    public string? ContentType { get; init; }
    public string? SortBy { get; init; }
    public bool SortDescending { get; init; } = true;
}

/// <summary>
/// Validator for GetImagesWithPaginationQuery.
/// </summary>
public sealed class GetImagesWithPaginationQueryValidator : AbstractValidator<GetImagesWithPaginationQuery>
{
    public GetImagesWithPaginationQueryValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1).WithMessage("Page number must be at least 1.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage("Page size must be between 1 and 100.");
    }
}

/// <summary>
/// Handler for GetImagesWithPaginationQuery.
/// </summary>
public sealed class GetImagesWithPaginationQueryHandler
    : IRequestHandler<GetImagesWithPaginationQuery, PaginatedList<ImageListDto>>
{
    private readonly IApplicationDbContext _context;

    public GetImagesWithPaginationQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedList<ImageListDto>> Handle(
        GetImagesWithPaginationQuery request,
        CancellationToken cancellationToken)
    {
        var query = _context.Images
            .Include(i => i.UploadedByUser)
            .AsNoTracking();

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchTerm = request.SearchTerm.ToLower().Trim();
            query = query.Where(i =>
                i.OriginalName.ToLower().Contains(searchTerm) ||
                i.FileName.ToLower().Contains(searchTerm) ||
                (i.AltText != null && i.AltText.ToLower().Contains(searchTerm)) ||
                (i.Caption != null && i.Caption.ToLower().Contains(searchTerm)));
        }

        // Apply folder filter
        if (request.FolderId.HasValue)
        {
            query = query.Where(i => i.FolderId == request.FolderId.Value);
        }

        // Apply content type filter
        if (!string.IsNullOrWhiteSpace(request.ContentType))
        {
            query = query.Where(i => i.ContentType.Contains(request.ContentType));
        }

        // Apply sorting
        query = ApplySorting(query, request.SortBy, request.SortDescending);

        // Project to DTO and paginate
        var projectedQuery = query.Select(i => new ImageListDto
        {
            Id = i.Id,
            FileName = i.FileName,
            OriginalName = i.OriginalName,
            ContentType = i.ContentType,
            Size = i.Size,
            Width = i.Width,
            Height = i.Height,
            AltText = i.AltText,
            Caption = i.Caption,
            HasThumbnail = i.ThumbnailData != null,
            HasMediumVersion = i.MediumData != null,
            FolderId = i.FolderId,
            CreatedAt = i.CreatedAt,
            ModifiedAt = i.LastModifiedAt,
            UploadedByUserId = i.UploadedByUserId,
            UploadedByUserName = i.UploadedByUser.FirstName + " " + i.UploadedByUser.LastName
        });

        return await PaginatedList<ImageListDto>.CreateAsync(
            projectedQuery,
            request.PageNumber,
            request.PageSize,
            cancellationToken);
    }

    private static IQueryable<CMS.Domain.Entities.ImageEntity> ApplySorting(
        IQueryable<CMS.Domain.Entities.ImageEntity> query,
        string? sortBy,
        bool sortDescending)
    {
        return sortBy?.ToLowerInvariant() switch
        {
            "name" or "originalname" => sortDescending
                ? query.OrderByDescending(i => i.OriginalName)
                : query.OrderBy(i => i.OriginalName),
            "size" => sortDescending
                ? query.OrderByDescending(i => i.Size)
                : query.OrderBy(i => i.Size),
            "contenttype" => sortDescending
                ? query.OrderByDescending(i => i.ContentType)
                : query.OrderBy(i => i.ContentType),
            "width" => sortDescending
                ? query.OrderByDescending(i => i.Width)
                : query.OrderBy(i => i.Width),
            "height" => sortDescending
                ? query.OrderByDescending(i => i.Height)
                : query.OrderBy(i => i.Height),
            _ => sortDescending
                ? query.OrderByDescending(i => i.CreatedAt)
                : query.OrderBy(i => i.CreatedAt)
        };
    }
}
