using CMS.Application.Common.Interfaces;
using CMS.Domain.Entities;
using MediatR;

namespace CMS.Application.Features.Media.Commands.UploadImage;

/// <summary>
/// Command to upload an image to the system.
/// </summary>
public sealed record UploadImageCommand : IRequest<UploadImageResponse>
{
    /// <summary>
    /// Image file data as byte array.
    /// </summary>
    public required byte[] FileData { get; init; }

    /// <summary>
    /// Original filename.
    /// </summary>
    public required string FileName { get; init; }

    /// <summary>
    /// Content type (MIME type).
    /// </summary>
    public required string ContentType { get; init; }

    /// <summary>
    /// Optional folder ID to organize the image.
    /// </summary>
    public int? FolderId { get; init; }

    /// <summary>
    /// Optional alt text for accessibility.
    /// </summary>
    public string? AltText { get; init; }

    /// <summary>
    /// Optional caption.
    /// </summary>
    public string? Caption { get; init; }

    /// <summary>
    /// Whether to generate thumbnail and medium versions.
    /// </summary>
    public bool GenerateVariants { get; init; } = true;
}

/// <summary>
/// Response containing uploaded image information.
/// </summary>
public sealed record UploadImageResponse
{
    public required int Id { get; init; }
    public required string FileName { get; init; }
    public required string OriginalName { get; init; }
    public required string ContentType { get; init; }
    public required long Size { get; init; }
    public required int Width { get; init; }
    public required int Height { get; init; }
    public string? AltText { get; init; }
    public string? Caption { get; init; }
    public bool HasThumbnail { get; init; }
    public bool HasMediumVersion { get; init; }
    public DateTime CreatedAt { get; init; }
}

/// <summary>
/// Handler for uploading images with automatic processing and variant generation.
/// </summary>
public sealed class UploadImageCommandHandler : IRequestHandler<UploadImageCommand, UploadImageResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IImageProcessingService _imageProcessingService;

    public UploadImageCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IImageProcessingService imageProcessingService)
    {
        _context = context;
        _currentUserService = currentUserService;
        _imageProcessingService = imageProcessingService;
    }

    public async Task<UploadImageResponse> Handle(UploadImageCommand request, CancellationToken cancellationToken)
    {
        // Validate image
        var isValid = await _imageProcessingService.IsValidImageAsync(request.FileData, cancellationToken);
        if (!isValid)
        {
            throw new ArgumentException("Invalid image file.", nameof(request.FileData));
        }

        // Get actual content type from image data
        var actualContentType = await _imageProcessingService.GetImageContentTypeAsync(request.FileData, cancellationToken);

        // Get image dimensions
        var (width, height) = await _imageProcessingService.GetImageDimensionsAsync(request.FileData, cancellationToken);

        // Optimize the original image
        var optimizedData = await _imageProcessingService.OptimizeImageAsync(request.FileData, 85, cancellationToken);

        // Generate unique filename
        var extension = Path.GetExtension(request.FileName);
        var uniqueFileName = $"{Guid.NewGuid()}{extension}";

        // Create image entity
        var image = ImageEntity.Create(
            fileName: uniqueFileName,
            originalName: request.FileName,
            contentType: actualContentType,
            size: optimizedData.Length,
            data: optimizedData,
            width: width,
            height: height,
            uploadedByUserId: _currentUserService.UserId!.Value,
            folderId: request.FolderId,
            altText: request.AltText,
            caption: request.Caption
        );

        // Generate variants if requested
        if (request.GenerateVariants)
        {
            // Generate thumbnail (150x150)
            var thumbnailData = await _imageProcessingService.GenerateThumbnailAsync(
                request.FileData,
                150,
                cancellationToken
            );
            image.SetThumbnail(thumbnailData);

            // Generate medium version (800x600)
            var mediumData = await _imageProcessingService.GenerateMediumVersionAsync(
                request.FileData,
                800,
                600,
                cancellationToken
            );
            image.SetMediumVersion(mediumData);
        }

        // Save to database
        await _context.Images.AddAsync(image, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return new UploadImageResponse
        {
            Id = image.Id,
            FileName = image.FileName,
            OriginalName = image.OriginalName,
            ContentType = image.ContentType,
            Size = image.Size,
            Width = image.Width,
            Height = image.Height,
            AltText = image.AltText,
            Caption = image.Caption,
            HasThumbnail = image.ThumbnailData != null,
            HasMediumVersion = image.MediumData != null,
            CreatedAt = image.CreatedAt
        };
    }
}
