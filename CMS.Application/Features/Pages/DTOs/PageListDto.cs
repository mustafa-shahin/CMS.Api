using CMS.Domain.Enums;

namespace CMS.Application.Features.Pages.DTOs;

/// <summary>
/// DTO for page list item
/// </summary>
public sealed class PageListDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public PageStatus Status { get; set; }
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
    public DateTime? PublishedAt { get; set; }
    public int Version { get; set; }
    public int? CreatedByUserId { get; set; }
    public string? CreatedByUserName { get; set; }
    public DateTime CreatedAt { get; set; }
    public int? LastModifiedByUserId { get; set; }
    public string? LastModifiedByUserName { get; set; }
    public DateTime? LastModifiedAt { get; set; }
}
