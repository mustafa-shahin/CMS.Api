using CMS.Domain.Common;

namespace CMS.Domain.Entities;

/// <summary>
/// Represents an image file with metadata for dimensions, alt text, and thumbnails.
/// </summary>
public sealed class ImageEntity : BaseAuditableEntity
{
    /// <summary>
    /// System-generated unique filename.
    /// </summary>
    public string FileName { get; private set; } = null!;

    /// <summary>
    /// Original filename as uploaded.
    /// </summary>
    public string OriginalName { get; private set; } = null!;

    /// <summary>
    /// MIME content type (image/jpeg, image/png, image/webp, etc.).
    /// </summary>
    public string ContentType { get; private set; } = null!;

    /// <summary>
    /// File size in bytes.
    /// </summary>
    public long Size { get; private set; }

    /// <summary>
    /// Storage path on the server for the original image (optional if using byte storage).
    /// </summary>
    public string? StoragePath { get; private set; }

    /// <summary>
    /// Public URL for accessing the original image.
    /// </summary>
    public string? PublicUrl { get; private set; }

    /// <summary>
    /// Image data stored as byte array in the database.
    /// Storing images in database provides better security, portability, and backup consistency.
    /// </summary>
    public byte[] Data { get; private set; } = null!;

    /// <summary>
    /// Image width in pixels.
    /// </summary>
    public int Width { get; private set; }

    /// <summary>
    /// Image height in pixels.
    /// </summary>
    public int Height { get; private set; }

    /// <summary>
    /// Alt text for accessibility.
    /// </summary>
    public string? AltText { get; private set; }

    /// <summary>
    /// Optional caption for the image.
    /// </summary>
    public string? Caption { get; private set; }

    /// <summary>
    /// Thumbnail storage path (small version) - optional if using byte storage.
    /// </summary>
    public string? ThumbnailPath { get; private set; }

    /// <summary>
    /// Public URL for the thumbnail.
    /// </summary>
    public string? ThumbnailUrl { get; private set; }

    /// <summary>
    /// Thumbnail image data as byte array.
    /// </summary>
    public byte[]? ThumbnailData { get; private set; }

    /// <summary>
    /// Medium-sized version storage path - optional if using byte storage.
    /// </summary>
    public string? MediumPath { get; private set; }

    /// <summary>
    /// Public URL for the medium-sized version.
    /// </summary>
    public string? MediumUrl { get; private set; }

    /// <summary>
    /// Medium-sized image data as byte array.
    /// </summary>
    public byte[]? MediumData { get; private set; }

    // Navigation properties
    public int? FolderId { get; private set; }
    public Folder? Folder { get; private set; }
    public int UploadedByUserId { get; private set; }
    public User UploadedByUser { get; private set; } = null!;

    /// <summary>
    /// Private constructor for EF Core.
    /// </summary>
    private ImageEntity() { }

    /// <summary>
    /// Creates a new image entity with byte array data.
    /// </summary>
    public static ImageEntity Create(
        string fileName,
        string originalName,
        string contentType,
        long size,
        byte[] data,
        int width,
        int height,
        int uploadedByUserId,
        int? folderId = null,
        string? altText = null,
        string? caption = null,
        string? storagePath = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);
        ArgumentException.ThrowIfNullOrWhiteSpace(originalName);
        ArgumentException.ThrowIfNullOrWhiteSpace(contentType);
        ArgumentNullException.ThrowIfNull(data);

        if (data.Length == 0)
            throw new ArgumentException("Image data cannot be empty.", nameof(data));

        if (size <= 0)
            throw new ArgumentOutOfRangeException(nameof(size), "Size must be greater than zero.");

        if (width <= 0)
            throw new ArgumentOutOfRangeException(nameof(width), "Width must be greater than zero.");

        if (height <= 0)
            throw new ArgumentOutOfRangeException(nameof(height), "Height must be greater than zero.");

        if (!contentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("Content type must be an image type.", nameof(contentType));

        return new ImageEntity
        {
            FileName = fileName,
            OriginalName = originalName,
            ContentType = contentType,
            Size = size,
            Data = data,
            StoragePath = storagePath,
            Width = width,
            Height = height,
            UploadedByUserId = uploadedByUserId,
            FolderId = folderId,
            AltText = altText,
            Caption = caption
        };
    }

    /// <summary>
    /// Moves the image to a different folder.
    /// </summary>
    public void MoveToFolder(int? folderId)
    {
        FolderId = folderId;
    }

    /// <summary>
    /// Sets the public URL for the original image.
    /// </summary>
    public void SetPublicUrl(string url)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(url);
        PublicUrl = url;
    }

    /// <summary>
    /// Updates alt text for accessibility.
    /// </summary>
    public void UpdateAltText(string? altText)
    {
        AltText = altText;
    }

    /// <summary>
    /// Updates the caption.
    /// </summary>
    public void UpdateCaption(string? caption)
    {
        Caption = caption;
    }

    /// <summary>
    /// Updates the main image data.
    /// </summary>
    public void SetData(byte[] data)
    {
        ArgumentNullException.ThrowIfNull(data);

        if (data.Length == 0)
            throw new ArgumentException("Image data cannot be empty.", nameof(data));

        Data = data;
        Size = data.Length;
    }

    /// <summary>
    /// Updates image dimensions (e.g., after processing or validation).
    /// </summary>
    public void UpdateDimensions(int width, int height)
    {
        if (width <= 0)
            throw new ArgumentOutOfRangeException(nameof(width), "Width must be greater than zero.");

        if (height <= 0)
            throw new ArgumentOutOfRangeException(nameof(height), "Height must be greater than zero.");

        Width = width;
        Height = height;
    }

    /// <summary>
    /// Sets thumbnail information with byte array data.
    /// </summary>
    public void SetThumbnail(byte[] thumbnailData, string? thumbnailPath = null, string? thumbnailUrl = null)
    {
        ArgumentNullException.ThrowIfNull(thumbnailData);

        if (thumbnailData.Length == 0)
            throw new ArgumentException("Thumbnail data cannot be empty.", nameof(thumbnailData));

        ThumbnailData = thumbnailData;
        ThumbnailPath = thumbnailPath;
        ThumbnailUrl = thumbnailUrl;
    }

    /// <summary>
    /// Sets medium-sized version information with byte array data.
    /// </summary>
    public void SetMediumVersion(byte[] mediumData, string? mediumPath = null, string? mediumUrl = null)
    {
        ArgumentNullException.ThrowIfNull(mediumData);

        if (mediumData.Length == 0)
            throw new ArgumentException("Medium version data cannot be empty.", nameof(mediumData));

        MediumData = mediumData;
        MediumPath = mediumPath;
        MediumUrl = mediumUrl;
    }

    /// <summary>
    /// Gets the aspect ratio of the image.
    /// </summary>
    public double GetAspectRatio()
    {
        return Height == 0 ? 0 : (double)Width / Height;
    }

    /// <summary>
    /// Checks if the image is a landscape orientation.
    /// </summary>
    public bool IsLandscape()
    {
        return Width > Height;
    }

    /// <summary>
    /// Checks if the image is a portrait orientation.
    /// </summary>
    public bool IsPortrait()
    {
        return Height > Width;
    }

    /// <summary>
    /// Checks if the image is square.
    /// </summary>
    public bool IsSquare()
    {
        return Width == Height;
    }
}
