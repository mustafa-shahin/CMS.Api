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
    public DateTime? LastModifiedAt { get; set; }
    public string? FolderName { get; set; }
}