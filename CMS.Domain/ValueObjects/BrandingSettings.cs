namespace CMS.Domain.ValueObjects;

/// <summary>
/// Value object representing site branding settings including logo, favicon, and site identity.
/// </summary>
public sealed record BrandingSettings
{
    /// <summary>
    /// Site name/title.
    /// </summary>
    public string SiteName { get; init; } = string.Empty;

    /// <summary>
    /// Site tagline or slogan.
    /// </summary>
    public string? Tagline { get; init; }

    /// <summary>
    /// ID of the logo image (light version).
    /// </summary>
    public int? LogoImageId { get; init; }

    /// <summary>
    /// ID of the logo image for dark mode (optional).
    /// </summary>
    public int? DarkLogoImageId { get; init; }

    /// <summary>
    /// ID of the favicon image.
    /// </summary>
    public int? FaviconImageId { get; init; }

    /// <summary>
    /// Logo alt text for accessibility.
    /// </summary>
    public string? LogoAltText { get; init; }

    /// <summary>
    /// Maximum logo height in pixels (for display sizing).
    /// </summary>
    public int MaxLogoHeight { get; init; }

    /// <summary>
    /// Whether to show the site name alongside the logo.
    /// </summary>
    public bool ShowSiteNameWithLogo { get; init; }

    /// <summary>
    /// Parameterless constructor for JSON deserialization.
    /// </summary>
    public BrandingSettings() { }

    public static BrandingSettings Create(
        string siteName,
        string? tagline = null,
        int? logoImageId = null,
        int? darkLogoImageId = null,
        int? faviconImageId = null,
        string? logoAltText = null,
        int maxLogoHeight = 48,
        bool showSiteNameWithLogo = true)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(siteName);

        if (maxLogoHeight < 16 || maxLogoHeight > 200)
            throw new ArgumentOutOfRangeException(nameof(maxLogoHeight), "Max logo height must be between 16 and 200 pixels.");

        return new BrandingSettings
        {
            SiteName = siteName.Trim(),
            Tagline = tagline?.Trim(),
            LogoImageId = logoImageId,
            DarkLogoImageId = darkLogoImageId,
            FaviconImageId = faviconImageId,
            LogoAltText = logoAltText?.Trim() ?? siteName,
            MaxLogoHeight = maxLogoHeight,
            ShowSiteNameWithLogo = showSiteNameWithLogo
        };
    }

    public static BrandingSettings CreateDefault()
    {
        return new BrandingSettings
        {
            SiteName = "My CMS Site",
            Tagline = null,
            LogoImageId = null,
            DarkLogoImageId = null,
            FaviconImageId = null,
            LogoAltText = "My CMS Site",
            MaxLogoHeight = 48,
            ShowSiteNameWithLogo = true
        };
    }

    /// <summary>
    /// Updates the logo configuration.
    /// </summary>
    public BrandingSettings WithLogo(int? logoImageId, string? logoAltText = null)
    {
        return this with
        {
            LogoImageId = logoImageId,
            LogoAltText = logoAltText ?? LogoAltText
        };
    }

    /// <summary>
    /// Updates the dark mode logo configuration.
    /// </summary>
    public BrandingSettings WithDarkLogo(int? darkLogoImageId)
    {
        return this with { DarkLogoImageId = darkLogoImageId };
    }

    /// <summary>
    /// Updates the favicon configuration.
    /// </summary>
    public BrandingSettings WithFavicon(int? faviconImageId)
    {
        return this with { FaviconImageId = faviconImageId };
    }

    /// <summary>
    /// Updates the site name and optionally the tagline.
    /// </summary>
    public BrandingSettings WithSiteIdentity(string siteName, string? tagline = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(siteName);

        return this with
        {
            SiteName = siteName.Trim(),
            Tagline = tagline?.Trim(),
            LogoAltText = LogoAltText ?? siteName
        };
    }
}
