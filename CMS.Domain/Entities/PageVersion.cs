using System.Text.Json;

namespace CMS.Domain.Entities;

/// <summary>
/// Represents a historical version of a page.
/// </summary>
public sealed class PageVersion
{
    /// <summary>
    /// Unique identifier for this version record.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// The page this version belongs to.
    /// </summary>
    public Guid PageId { get; private set; }

    /// <summary>
    /// Version number at the time of snapshot.
    /// </summary>
    public int Version { get; private set; }

    /// <summary>
    /// Page title at the time of snapshot.
    /// </summary>
    public string Title { get; private set; } = null!;

    /// <summary>
    /// Page components at the time of snapshot.
    /// </summary>
    public JsonDocument? Components { get; private set; }

    /// <summary>
    /// Notes describing the changes in this version.
    /// </summary>
    public string? ChangeNotes { get; private set; }

    /// <summary>
    /// User who created this version snapshot.
    /// </summary>
    public int CreatedByUserId { get; private set; }

    /// <summary>
    /// Timestamp when this version was created.
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    // Navigation properties
    public Page Page { get; private set; } = null!;
    public User CreatedByUser { get; private set; } = null!;

    /// <summary>
    /// Private constructor for EF Core.
    /// </summary>
    private PageVersion() { }

    /// <summary>
    /// Creates a new page version snapshot.
    /// </summary>
    public static PageVersion Create(Page page, int userId, string? changeNotes = null)
    {
        return new PageVersion
        {
            Id = Guid.NewGuid(),
            PageId = page.Id,
            Version = page.Version,
            Title = page.Title,
            Components = page.Components,
            ChangeNotes = changeNotes?.Trim(),
            CreatedByUserId = userId,
            CreatedAt = DateTime.UtcNow
        };
    }
}