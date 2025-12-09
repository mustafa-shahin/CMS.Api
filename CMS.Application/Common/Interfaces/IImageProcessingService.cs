namespace CMS.Application.Common.Interfaces;

/// <summary>
/// Service for image processing operations including resizing, thumbnail generation, and compression.
/// </summary>
public interface IImageProcessingService
{
    /// <summary>
    /// Get image dimensions from byte array.
    /// </summary>
    Task<(int width, int height)> GetImageDimensionsAsync(byte[] imageData, CancellationToken cancellationToken = default);

    /// <summary>
    /// Resize an image to specified dimensions while maintaining aspect ratio.
    /// </summary>
    Task<byte[]> ResizeImageAsync(
        byte[] imageData,
        int maxWidth,
        int maxHeight,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generate a thumbnail from an image.
    /// </summary>
    Task<byte[]> GenerateThumbnailAsync(
        byte[] imageData,
        int thumbnailSize = 150,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generate a medium-sized version of an image.
    /// </summary>
    Task<byte[]> GenerateMediumVersionAsync(
        byte[] imageData,
        int maxWidth = 800,
        int maxHeight = 600,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Optimize/compress an image while maintaining quality.
    /// </summary>
    Task<byte[]> OptimizeImageAsync(
        byte[] imageData,
        int quality = 85,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validate if the byte array contains a valid image.
    /// </summary>
    Task<bool> IsValidImageAsync(byte[] imageData, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get the content type (MIME type) of an image.
    /// </summary>
    Task<string> GetImageContentTypeAsync(byte[] imageData, CancellationToken cancellationToken = default);
}
