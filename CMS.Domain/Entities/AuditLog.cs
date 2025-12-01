using System.Text.Json;

namespace CMS.Domain.Entities;

/// <summary>
/// Represents an audit log entry for tracking sensitive operations.
/// </summary>
public sealed class AuditLog
{
    /// <summary>
    /// Unique identifier for the audit log entry.
    /// </summary>
    public int Id { get; private set; }

    /// <summary>
    /// The user who performed the action (null for anonymous actions).
    /// </summary>
    public int? UserId { get; private set; }

    /// <summary>
    /// The action performed (e.g., "Login", "Create", "Update", "Delete").
    /// </summary>
    public string Action { get; private set; } = null!;

    /// <summary>
    /// The type of entity affected.
    /// </summary>
    public string EntityType { get; private set; } = null!;

    /// <summary>
    /// The ID of the entity affected (if applicable).
    /// </summary>
    public int? EntityId { get; private set; }

    /// <summary>
    /// JSON representation of old values before the change.
    /// </summary>
    public JsonDocument? OldValues { get; private set; }

    /// <summary>
    /// JSON representation of new values after the change.
    /// </summary>
    public JsonDocument? NewValues { get; private set; }

    /// <summary>
    /// IP address from which the action was performed.
    /// </summary>
    public string? IpAddress { get; private set; }

    /// <summary>
    /// User agent string of the client.
    /// </summary>
    public string? UserAgent { get; private set; }

    /// <summary>
    /// UTC timestamp when the action was performed.
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// Additional context or notes about the action.
    /// </summary>
    public string? AdditionalInfo { get; private set; }

    /// <summary>
    /// Private constructor for EF Core.
    /// </summary>
    private AuditLog() { }

    /// <summary>
    /// Creates a new audit log entry.
    /// </summary>
    public static AuditLog Create(
        int? userId,
        string action,
        string entityType,
        int? entityId = null,
        JsonDocument? oldValues = null,
        JsonDocument? newValues = null,
        string? ipAddress = null,
        string? userAgent = null,
        string? additionalInfo = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(action);
        ArgumentException.ThrowIfNullOrWhiteSpace(entityType);

        return new AuditLog
        {
            UserId = userId,
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            OldValues = oldValues,
            NewValues = newValues,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            AdditionalInfo = additionalInfo,
            CreatedAt = DateTime.UtcNow
        };
    }
}