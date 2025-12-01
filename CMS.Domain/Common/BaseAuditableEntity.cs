namespace CMS.Domain.Common;

/// <summary>
/// Base class for all auditable entities that tracks creation and modification metadata.
/// Inherits from BaseEntity and implements IAuditableEntity for automatic audit field population.
/// </summary>
public abstract class BaseAuditableEntity : BaseEntity, IAuditableEntity
{
    /// <summary>
    /// The ID of the user who created this entity.
    /// </summary>
    public int? CreatedBy { get; set; }

    /// <summary>
    /// The UTC date and time when this entity was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// The ID of the user who last modified this entity.
    /// </summary>
    public int? LastModifiedBy { get; set; }

    /// <summary>
    /// The UTC date and time when this entity was last modified.
    /// </summary>
    public DateTime? LastModifiedAt { get; set; }
}