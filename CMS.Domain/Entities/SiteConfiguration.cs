using CMS.Domain.Common;
using CMS.Domain.Enums;
using System.Text.Json;

namespace CMS.Domain.Entities;

/// <summary>
/// Represents a site configuration setting.
/// </summary>
public sealed class SiteConfiguration : BaseAuditableEntity
{
    /// <summary>
    /// Configuration key.
    /// </summary>
    public string Key { get; private set; } = null!;

    /// <summary>
    /// Configuration value as JSON.
    /// </summary>
    public JsonDocument Value { get; private set; } = null!;

    /// <summary>
    /// Configuration category.
    /// </summary>
    public ConfigurationCategory Category { get; private set; }

    /// <summary>
    /// User who last updated this configuration.
    /// </summary>
    public int? UpdatedByUserId { get; private set; }
    public User? UpdatedByUser { get; private set; }

    /// <summary>
    /// Private constructor for EF Core.
    /// </summary>
    private SiteConfiguration() { }

    /// <summary>
    /// Creates a new site configuration.
    /// </summary>
    public static SiteConfiguration Create(string key, JsonDocument value, ConfigurationCategory category)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentNullException.ThrowIfNull(value);

        return new SiteConfiguration
        {
            Key = key.Trim(),
            Value = value,
            Category = category
        };
    }

    /// <summary>
    /// Updates the configuration value.
    /// </summary>
    public void UpdateValue(JsonDocument value, int updatedByUserId)
    {
        ArgumentNullException.ThrowIfNull(value);

        Value = value;
        UpdatedByUserId = updatedByUserId;
    }
}