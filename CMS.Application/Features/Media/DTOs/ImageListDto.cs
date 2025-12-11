namespace CMS.Application.Features.Media.DTOs;

/// <summary>
/// DTO for image list items in paginated results.
/// </summary>
public sealed record ImageListDto
{
    public int Id { get; init; }
    public string FileName { get; init; } = null!;
    public string OriginalName { get; init; } = null!;
    public string ContentType { get; init; } = null!;
    public long Size { get; init; }
    public int Width { get; init; }
    public int Height { get; init; }
    public string? AltText { get; init; }
    public string? Caption { get; init; }
    public bool HasThumbnail { get; init; }
    public bool HasMediumVersion { get; init; }
    public int? FolderId { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? ModifiedAt { get; init; }
    public int UploadedByUserId { get; init; }
    public string? UploadedByUserName { get; init; }
/// DTO for image list item
/// </summary>
public sealed class ImageListDto
{
    public int Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string OriginalName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long Size { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public string? AltText { get; set; }
    public string? Caption { get; set; }
    public bool HasThumbnail { get; set; }
    public bool HasMediumVersion { get; set; }
    public int? FolderId { get; set; }
    public string? FolderName { get; set; }
    public int? UploadedByUserId { get; set; }
    public string? UploadedByUserName { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastModifiedAt { get; set; }
}
