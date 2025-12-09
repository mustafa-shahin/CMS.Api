using CMS.Domain.Common;
using CMS.Domain.ValueObjects;

namespace CMS.Domain.Entities;

/// <summary>
/// Aggregate root representing site customization settings including theme, typography, and layout.
/// Uses value objects for complex configuration data stored as JSONB in PostgreSQL.
/// </summary>
public sealed class CustomizationSettings : BaseAuditableEntity
{
    /// <summary>
    /// Indicates whether this is the active customization configuration.
    /// Only one configuration should be active at a time per tenant.
    /// </summary>
    public bool IsActive { get; private set; }

    /// <summary>
    /// Version number for optimistic concurrency control.
    /// </summary>
    public int Version { get; private set; }

    /// <summary>
    /// Theme configuration including all color palettes.
    /// </summary>
    public ThemeSettings ThemeConfiguration { get; private set; } = null!;

    /// <summary>
    /// Typography configuration including font families and text styles.
    /// </summary>
    public TypographySettings TypographyConfiguration { get; private set; } = null!;

    /// <summary>
    /// Layout configuration including header, footer, and spacing options.
    /// </summary>
    public LayoutSettings LayoutConfiguration { get; private set; } = null!;

    /// <summary>
    /// Branding configuration including logo, favicon, and site identity.
    /// </summary>
    public BrandingSettings BrandingConfiguration { get; private set; } = null!;

    /// <summary>
    /// Navigation property to the user who last updated this configuration.
    /// </summary>
    public User? UpdatedByUser { get; private set; }

    /// <summary>
    /// Private constructor for EF Core.
    /// </summary>
    private CustomizationSettings() { }

    /// <summary>
    /// Creates a new customization settings instance with all configurations.
    /// </summary>
    /// <param name="themeConfiguration">Theme settings.</param>
    /// <param name="typographyConfiguration">Typography settings.</param>
    /// <param name="layoutConfiguration">Layout settings.</param>
    /// <param name="brandingConfiguration">Branding settings.</param>
    /// <param name="isActive">Whether this should be the active configuration.</param>
    /// <returns>A new CustomizationSettings instance.</returns>
    public static CustomizationSettings Create(
        ThemeSettings themeConfiguration,
        TypographySettings typographyConfiguration,
        LayoutSettings layoutConfiguration,
        BrandingSettings brandingConfiguration,
        bool isActive = true)
    {
        ArgumentNullException.ThrowIfNull(themeConfiguration, nameof(themeConfiguration));
        ArgumentNullException.ThrowIfNull(typographyConfiguration, nameof(typographyConfiguration));
        ArgumentNullException.ThrowIfNull(layoutConfiguration, nameof(layoutConfiguration));
        ArgumentNullException.ThrowIfNull(brandingConfiguration, nameof(brandingConfiguration));

        return new CustomizationSettings
        {
            ThemeConfiguration = themeConfiguration,
            TypographyConfiguration = typographyConfiguration,
            LayoutConfiguration = layoutConfiguration,
            BrandingConfiguration = brandingConfiguration,
            IsActive = isActive,
            Version = 1
        };
    }

    /// <summary>
    /// Creates a new customization settings instance with default values.
    /// </summary>
    public static CustomizationSettings CreateDefault()
    {
        return new CustomizationSettings
        {
            ThemeConfiguration = ThemeSettings.CreateDefault(),
            TypographyConfiguration = TypographySettings.CreateDefault(),
            LayoutConfiguration = LayoutSettings.CreateDefault(),
            BrandingConfiguration = BrandingSettings.CreateDefault(),
            IsActive = true,
            Version = 1
        };
    }

    /// <summary>
    /// Updates the theme configuration.
    /// </summary>
    /// <param name="themeConfiguration">New theme configuration.</param>
    /// <param name="updatedByUserId">ID of the user making the update.</param>
    public void UpdateTheme(ThemeSettings themeConfiguration, int updatedByUserId)
    {
        ArgumentNullException.ThrowIfNull(themeConfiguration, nameof(themeConfiguration));

        ThemeConfiguration = themeConfiguration;
        LastModifiedBy = updatedByUserId;
        LastModifiedAt = DateTime.UtcNow;
        Version++;
    }

    /// <summary>
    /// Updates the typography configuration.
    /// </summary>
    /// <param name="typographyConfiguration">New typography configuration.</param>
    /// <param name="updatedByUserId">ID of the user making the update.</param>
    public void UpdateTypography(TypographySettings typographyConfiguration, int updatedByUserId)
    {
        ArgumentNullException.ThrowIfNull(typographyConfiguration, nameof(typographyConfiguration));

        TypographyConfiguration = typographyConfiguration;
        LastModifiedBy = updatedByUserId;
        LastModifiedAt = DateTime.UtcNow;
        Version++;
    }

    /// <summary>
    /// Updates the layout configuration.
    /// </summary>
    /// <param name="layoutConfiguration">New layout configuration.</param>
    /// <param name="updatedByUserId">ID of the user making the update.</param>
    public void UpdateLayout(LayoutSettings layoutConfiguration, int updatedByUserId)
    {
        ArgumentNullException.ThrowIfNull(layoutConfiguration, nameof(layoutConfiguration));

        LayoutConfiguration = layoutConfiguration;
        LastModifiedBy = updatedByUserId;
        LastModifiedAt = DateTime.UtcNow;
        Version++;
    }

    /// <summary>
    /// Updates the branding configuration.
    /// </summary>
    /// <param name="brandingConfiguration">New branding configuration.</param>
    /// <param name="updatedByUserId">ID of the user making the update.</param>
    public void UpdateBranding(BrandingSettings brandingConfiguration, int updatedByUserId)
    {
        ArgumentNullException.ThrowIfNull(brandingConfiguration, nameof(brandingConfiguration));

        BrandingConfiguration = brandingConfiguration;
        LastModifiedBy = updatedByUserId;
        LastModifiedAt = DateTime.UtcNow;
        Version++;
    }

    /// <summary>
    /// Updates all configurations at once.
    /// </summary>
    public void UpdateAll(
        ThemeSettings themeConfiguration,
        TypographySettings typographyConfiguration,
        LayoutSettings layoutConfiguration,
        BrandingSettings brandingConfiguration,
        int updatedByUserId)
    {
        ArgumentNullException.ThrowIfNull(themeConfiguration, nameof(themeConfiguration));
        ArgumentNullException.ThrowIfNull(typographyConfiguration, nameof(typographyConfiguration));
        ArgumentNullException.ThrowIfNull(layoutConfiguration, nameof(layoutConfiguration));
        ArgumentNullException.ThrowIfNull(brandingConfiguration, nameof(brandingConfiguration));

        ThemeConfiguration = themeConfiguration;
        TypographyConfiguration = typographyConfiguration;
        LayoutConfiguration = layoutConfiguration;
        BrandingConfiguration = brandingConfiguration;
        LastModifiedBy = updatedByUserId;
        LastModifiedAt = DateTime.UtcNow;
        Version++;
    }

    /// <summary>
    /// Activates this configuration and marks it as the active one.
    /// </summary>
    public void Activate()
    {
        IsActive = true;
    }

    /// <summary>
    /// Deactivates this configuration.
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
    }

    /// <summary>
    /// Resets all configurations to their default values.
    /// </summary>
    /// <param name="updatedByUserId">ID of the user performing the reset.</param>
    public void ResetToDefaults(int updatedByUserId)
    {
        ThemeConfiguration = ThemeSettings.CreateDefault();
        TypographyConfiguration = TypographySettings.CreateDefault();
        LayoutConfiguration = LayoutSettings.CreateDefault();
        BrandingConfiguration = BrandingSettings.CreateDefault();
        LastModifiedBy = updatedByUserId;
        LastModifiedAt = DateTime.UtcNow;
        Version++;
    }
}
