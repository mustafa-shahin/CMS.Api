namespace CMS.Domain.Common;

/// <summary>
/// Base class for all auditable entities that tracks creation and modification metadata.
/// </summary>
public abstract class BaseAuditableEntity : BaseEntity
{
    /// <summary>
    /// The ID of the user who created this entity.
    /// </summary>
    public Guid? CreatedBy { get; set; }

    /// <summary>
    /// The UTC date and time when this entity was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// The ID of the user who last modified this entity.
    /// </summary>
    public Guid? LastModifiedBy { get; set; }

    /// <summary>
    /// The UTC date and time when this entity was last modified.
    /// </summary>
    public DateTime? LastModifiedAt { get; set; }
}