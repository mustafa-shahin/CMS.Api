using CMS.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CMS.Application.Features.Media.Queries.GetImage;

/// <summary>
/// Query to retrieve image data by ID.
/// </summary>
public sealed record GetImageQuery : IRequest<GetImageResponse?>
{
    /// <summary>
    /// Image ID.
    /// </summary>
    public required int ImageId { get; init; }

    /// <summary>
    /// Image variant to retrieve (original, thumbnail, or medium).
    /// </summary>
    public ImageVariant Variant { get; init; } = ImageVariant.Original;
}

/// <summary>
/// Image variant types.
/// </summary>
public enum ImageVariant
{
    Original,
    Thumbnail,
    Medium
}

/// <summary>
/// Response containing image data and metadata.
/// </summary>
public sealed record GetImageResponse
{
    public required byte[] Data { get; init; }
    public required string ContentType { get; init; }
    public required string FileName { get; init; }
    public required int Width { get; init; }
    public required int Height { get; init; }
    public DateTime? LastModified { get; init; }
}

/// <summary>
/// Handler for retrieving image data.
/// </summary>
public sealed class GetImageQueryHandler : IRequestHandler<GetImageQuery, GetImageResponse?>
{
    private readonly IApplicationDbContext _context;

    public GetImageQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<GetImageResponse?> Handle(GetImageQuery request, CancellationToken cancellationToken)
    {
        var image = await _context.Images
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.Id == request.ImageId, cancellationToken);

        if (image == null)
        {
            return null;
        }

        (byte[] data, string contentType) = request.Variant switch
        {
            ImageVariant.Thumbnail when image.ThumbnailData != null =>
                (image.ThumbnailData, "image/webp"),
            ImageVariant.Medium when image.MediumData != null =>
                (image.MediumData, image.ContentType),
            _ => (image.Data, image.ContentType)
        };

        return new GetImageResponse
        {
            Data = data,
            ContentType = contentType,
            FileName = image.FileName,
            Width = image.Width,
            Height = image.Height,
            LastModified = image.LastModifiedAt ?? image.CreatedAt
        };
    }
}
