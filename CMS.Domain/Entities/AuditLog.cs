using System.Text.Json;

namespace CMS.Domain.Entities
{
    public sealed class AuditLog
    {
        public Guid Id { get; private set; }
        public Guid? UserId { get; private set; }
        public string Action { get; private set; } = null!;
        public string EntityType { get; private set; } = null!;
        public Guid? EntityId { get; private set; }
        public JsonDocument? OldValues { get; private set; }
        public JsonDocument? NewValues { get; private set; }
        public string? IpAddress { get; private set; }
        public string? UserAgent { get; private set; }
        public DateTime CreatedAt { get; private set; }

        public static AuditLog Create(Guid? userId, string action, string entityType, Guid? entityId, JsonDocument? oldValues, JsonDocument? newValues, string? ipAddress, string? userAgent)
        {
            return new AuditLog
            {
                Id = Guid.CreateVersion7(),
                UserId = userId,
                Action = action,
                EntityType = entityType,
                EntityId = entityId,
                OldValues = oldValues,
                NewValues = newValues,
                IpAddress = ipAddress,
                UserAgent = userAgent,
                CreatedAt = DateTime.UtcNow
            };
        }
    }
}
