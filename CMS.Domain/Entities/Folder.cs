using CMS.Domain.Common;

namespace CMS.Domain.Entities;

/// <summary>
/// Represents a folder in the file management system.
/// </summary>
public sealed class Folder : BaseAuditableEntity
{
    /// <summary>
    /// Folder name.
    /// </summary>
    public string Name { get; private set; } = null!;

    /// <summary>
    /// Parent folder ID (null for root folders).
    /// </summary>
    public int? ParentId { get; private set; }

    // Navigation properties
    public Folder? Parent { get; private set; }
    public ICollection<Folder> Children { get; private set; } = new List<Folder>();
    public ICollection<FileEntity> Files { get; private set; } = new List<FileEntity>();
    public int CreatedByUserId { get; private set; }
    public User CreatedByUser { get; private set; } = null!;

    /// <summary>
    /// Private constructor for EF Core.
    /// </summary>
    private Folder() { }

    /// <summary>
    /// Creates a new folder.
    /// </summary>
    public static Folder Create(string name, int createdByUserId, int? parentId = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return new Folder
        {
            Name = name.Trim(),
            ParentId = parentId,
            CreatedByUserId = createdByUserId
        };
    }

    /// <summary>
    /// Renames the folder.
    /// </summary>
    public void Rename(string newName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(newName);
        Name = newName.Trim();
    }

    /// <summary>
    /// Moves the folder to a new parent.
    /// </summary>
    public void MoveTo(int? newParentId)
    {
        ParentId = newParentId;
    }
}