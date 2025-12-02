using CMS.Domain.Common;
using CMS.Domain.Enums;
using System.Text.Json;

namespace CMS.Domain.Entities;

/// <summary>
/// Represents a CMS page with versioning support.
/// </summary>
public sealed class Page : BaseAuditableEntity
{
    /// <summary>
    /// Page title.
    /// </summary>
    public string Title { get; private set; } = null!;

    /// <summary>
    /// URL-friendly slug for the page.
    /// </summary>
    public string Slug { get; private set; } = null!;

    /// <summary>
    /// Current publication status.
    /// </summary>
    public PageStatus Status { get; private set; }

    /// <summary>
    /// JSON document containing page components/content.
    /// </summary>
    public JsonDocument? Components { get; private set; }

    /// <summary>
    /// SEO meta title.
    /// </summary>
    public string? MetaTitle { get; private set; }

    /// <summary>
    /// SEO meta description.
    /// </summary>
    public string? MetaDescription { get; private set; }

    /// <summary>
    /// Timestamp when the page was published.
    /// </summary>
    public DateTime? PublishedAt { get; private set; }

    /// <summary>
    /// Current version number.
    /// </summary>
    public int Version { get; private set; }

    // Navigation properties
    public int CreatedByUserId { get; private set; }
    public User CreatedByUser { get; private set; } = null!;
    public int? UpdatedByUserId { get; private set; }
    public User? UpdatedByUser { get; private set; }
    public ICollection<PageVersion> Versions { get; private set; } = new List<PageVersion>();

    /// <summary>
    /// Private constructor for EF Core.
    /// </summary>
    private Page() { }

    /// <summary>
    /// Creates a new page.
    /// </summary>
    public static Page Create(string title, string slug, int createdByUserId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentException.ThrowIfNullOrWhiteSpace(slug);

        return new Page
        {
            Title = title.Trim(),
            Slug = slug.ToLowerInvariant().Trim(),
            Status = PageStatus.Draft,
            Version = 1,
            CreatedByUserId = createdByUserId
        };
    }

    /// <summary>
    /// Updates the page content and metadata.
    /// </summary>
    public void Update(
        string title,
        string slug,
        JsonDocument? components,
        string? metaTitle,
        string? metaDescription,
        int updatedByUserId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentException.ThrowIfNullOrWhiteSpace(slug);

        Title = title.Trim();
        Slug = slug.ToLowerInvariant().Trim();
        Components = components;
        MetaTitle = metaTitle?.Trim();
        MetaDescription = metaDescription?.Trim();
        UpdatedByUserId = updatedByUserId;
        Version++;
    }

    /// <summary>
    /// Publishes the page.
    /// </summary>
    public void Publish(int publishedByUserId)
    {
        Status = PageStatus.Published;
        PublishedAt = DateTime.UtcNow;
        UpdatedByUserId = publishedByUserId;
    }

    /// <summary>
    /// Unpublishes the page (returns to draft).
    /// </summary>
    public void Unpublish()
    {
        Status = PageStatus.Draft;
        PublishedAt = null;
    }

    /// <summary>
    /// Archives the page.
    /// </summary>
    public void Archive()
    {
        Status = PageStatus.Archived;
    }

    /// <summary>
    /// Creates a version snapshot of the current page state.
    /// </summary>
    public PageVersion CreateVersionSnapshot(int userId, string? changeNotes = null)
    {
        return PageVersion.Create(this, userId, changeNotes);
    }
}