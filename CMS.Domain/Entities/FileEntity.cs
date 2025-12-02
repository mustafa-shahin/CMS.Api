using CMS.Domain.Common;

namespace CMS.Domain.Entities;

/// <summary>
/// Represents a file uploaded to the CMS.
/// </summary>
public sealed class FileEntity : BaseAuditableEntity
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
    /// MIME content type.
    /// </summary>
    public string ContentType { get; private set; } = null!;

    /// <summary>
    /// File size in bytes.
    /// </summary>
    public long Size { get; private set; }

    /// <summary>
    /// Storage path on the server.
    /// </summary>
    public string StoragePath { get; private set; } = null!;

    /// <summary>
    /// Public URL for accessing the file.
    /// </summary>
    public string? PublicUrl { get; private set; }

    // Navigation properties
    public int? FolderId { get; private set; }
    public Folder? Folder { get; private set; }
    public int UploadedByUserId { get; private set; }
    public User UploadedByUser { get; private set; } = null!;

    /// <summary>
    /// Private constructor for EF Core.
    /// </summary>
    private FileEntity() { }

    /// <summary>
    /// Creates a new file entity.
    /// </summary>
    public static FileEntity Create(
        string fileName,
        string originalName,
        string contentType,
        long size,
        string storagePath,
        int uploadedByUserId,
        int? folderId = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);
        ArgumentException.ThrowIfNullOrWhiteSpace(originalName);
        ArgumentException.ThrowIfNullOrWhiteSpace(contentType);
        ArgumentException.ThrowIfNullOrWhiteSpace(storagePath);

        return new FileEntity
        {
            FileName = fileName,
            OriginalName = originalName,
            ContentType = contentType,
            Size = size,
            StoragePath = storagePath,
            UploadedByUserId = uploadedByUserId,
            FolderId = folderId
        };
    }

    /// <summary>
    /// Moves the file to a different folder.
    /// </summary>
    public void MoveToFolder(int? folderId)
    {
        FolderId = folderId;
    }

    /// <summary>
    /// Sets the public URL for the file.
    /// </summary>
    public void SetPublicUrl(string url)
    {
        PublicUrl = url;
    }
}