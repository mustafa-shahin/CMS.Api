using CMS.Application.Common.Interfaces;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;

namespace CMS.Infrastructure.Services;

/// <summary>
/// Image processing service using SixLabors.ImageSharp.
/// </summary>
public sealed class ImageProcessingService : IImageProcessingService
{
    private static readonly HashSet<string> AllowedContentTypes = new()
    {
        "image/jpeg",
        "image/jpg",
        "image/png",
        "image/webp",
        "image/gif"
    };

    /// <inheritdoc />
    public async Task<(int width, int height)> GetImageDimensionsAsync(
        byte[] imageData,
        CancellationToken cancellationToken = default)
    {
        using var image = await Image.LoadAsync(new MemoryStream(imageData), cancellationToken);
        return (image.Width, image.Height);
    }

    /// <inheritdoc />
    public async Task<byte[]> ResizeImageAsync(
        byte[] imageData,
        int maxWidth,
        int maxHeight,
        CancellationToken cancellationToken = default)
    {
        using var image = await Image.LoadAsync(new MemoryStream(imageData), cancellationToken);
        using var outputStream = new MemoryStream();

        // Calculate new dimensions maintaining aspect ratio
        var ratioX = (double)maxWidth / image.Width;
        var ratioY = (double)maxHeight / image.Height;
        var ratio = Math.Min(ratioX, ratioY);

        var newWidth = (int)(image.Width * ratio);
        var newHeight = (int)(image.Height * ratio);

        // Resize the image
        image.Mutate(x => x.Resize(new ResizeOptions
        {
            Size = new Size(newWidth, newHeight),
            Mode = ResizeMode.Max,
            Sampler = KnownResamplers.Lanczos3
        }));

        // Save with appropriate encoder
        var encoder = GetEncoder(image.Metadata.DecodedImageFormat);
        await image.SaveAsync(outputStream, encoder, cancellationToken);

        return outputStream.ToArray();
    }

    /// <inheritdoc />
    public async Task<byte[]> GenerateThumbnailAsync(
        byte[] imageData,
        int thumbnailSize = 150,
        CancellationToken cancellationToken = default)
    {
        using var image = await Image.LoadAsync(new MemoryStream(imageData), cancellationToken);
        using var outputStream = new MemoryStream();

        // Generate square thumbnail with crop
        image.Mutate(x => x.Resize(new ResizeOptions
        {
            Size = new Size(thumbnailSize, thumbnailSize),
            Mode = ResizeMode.Crop,
            Sampler = KnownResamplers.Lanczos3
        }));

        // Use WebP for thumbnails for better compression
        var encoder = new WebpEncoder { Quality = 80 };
        await image.SaveAsync(outputStream, encoder, cancellationToken);

        return outputStream.ToArray();
    }

    /// <inheritdoc />
    public async Task<byte[]> GenerateMediumVersionAsync(
        byte[] imageData,
        int maxWidth = 800,
        int maxHeight = 600,
        CancellationToken cancellationToken = default)
    {
        return await ResizeImageAsync(imageData, maxWidth, maxHeight, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<byte[]> OptimizeImageAsync(
        byte[] imageData,
        int quality = 85,
        CancellationToken cancellationToken = default)
    {
        using var image = await Image.LoadAsync(new MemoryStream(imageData), cancellationToken);
        using var outputStream = new MemoryStream();

        // Get the original format or default to JPEG
        var format = image.Metadata.DecodedImageFormat;

        IImageEncoder encoder = format?.Name?.ToLowerInvariant() switch
        {
            "png" => new PngEncoder { CompressionLevel = PngCompressionLevel.BestCompression },
            "webp" => new WebpEncoder { Quality = quality },
            _ => new JpegEncoder { Quality = quality }
        };

        await image.SaveAsync(outputStream, encoder, cancellationToken);
        return outputStream.ToArray();
    }

    /// <inheritdoc />
    public async Task<bool> IsValidImageAsync(
        byte[] imageData,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var image = await Image.LoadAsync(new MemoryStream(imageData), cancellationToken);
            return image.Width > 0 && image.Height > 0;
        }
        catch
        {
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<string> GetImageContentTypeAsync(
        byte[] imageData,
        CancellationToken cancellationToken = default)
    {
        using var image = await Image.LoadAsync(new MemoryStream(imageData), cancellationToken);
        var format = image.Metadata.DecodedImageFormat;

        return format?.Name?.ToLowerInvariant() switch
        {
            "png" => "image/png",
            "jpeg" or "jpg" => "image/jpeg",
            "webp" => "image/webp",
            "gif" => "image/gif",
            _ => "application/octet-stream"
        };
    }

    /// <summary>
    /// Get the appropriate encoder for the image format.
    /// </summary>
    private static IImageEncoder GetEncoder(IImageFormat? format)
    {
        return format?.Name?.ToLowerInvariant() switch
        {
            "png" => new PngEncoder { CompressionLevel = PngCompressionLevel.BestCompression },
            "webp" => new WebpEncoder { Quality = 85 },
            _ => new JpegEncoder { Quality = 85 }
        };
    }
}
