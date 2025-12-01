using CMS.Domain.Common;
using CMS.Domain.Enums;
using System.Text.Json;

namespace CMS.Domain.Entities
{
    public sealed class SiteConfiguration : BaseAuditableEntity
    {
        public Guid Id { get; private set; }
        public string Key { get; private set; } = null!;
        public JsonDocument Value { get; private set; } = null!;
        public ConfigurationCategory Category { get; private set; }
        public Guid? UpdatedByUserId { get; private set; }
        public User? UpdatedByUser { get; private set; }

        public static SiteConfiguration Create(string key, JsonDocument value, ConfigurationCategory category)
        {
            return new SiteConfiguration
            {
                Id = Guid.CreateVersion7(),
                Key = key,
                Value = value,
                Category = category
            };
        }

        public void UpdateValue(JsonDocument value, Guid updatedByUserId)
        {
            Value = value;
            UpdatedByUserId = updatedByUserId;
        }
    }

}
