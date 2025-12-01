namespace CMS.Domain.Common;

/// <summary>
/// Marker interface for domain events.
/// </summary>
public interface IDomainEvent
{
    DateTime OccurredOn { get; }
}

/// <summary>
/// Base interface for all entities requiring audit fields.
/// </summary>
public interface IAuditableEntity
{
    int? CreatedBy { get; set; }
    DateTime CreatedAt { get; set; }
    int? LastModifiedBy { get; set; }
    DateTime? LastModifiedAt { get; set; }
}