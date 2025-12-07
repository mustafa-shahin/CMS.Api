using CMS.Domain.Enums;

namespace CMS.Domain.ValueObjects;

/// <summary>
/// Value object representing header configuration options.
/// </summary>
public sealed record HeaderOptions
{
    public HeaderTemplate Template { get; init; }
    public Placement LogoPlacement { get; init; }
    public bool ShowSearch { get; init; }
    public bool StickyHeader { get; init; }

    private HeaderOptions() { }

    public static HeaderOptions Create(
        HeaderTemplate template,
        Placement logoPlacement,
        bool showSearch,
        bool stickyHeader)
    {
        return new HeaderOptions
        {
            Template = template,
            LogoPlacement = logoPlacement,
            ShowSearch = showSearch,
            StickyHeader = stickyHeader
        };
    }

    public static HeaderOptions CreateDefault()
    {
        return new HeaderOptions
        {
            Template = HeaderTemplate.Standard,
            LogoPlacement = Placement.Left,
            ShowSearch = true,
            StickyHeader = true
        };
    }
}

/// <summary>
/// Value object representing footer configuration options.
/// </summary>
public sealed record FooterOptions
{
    public FooterTemplate Template { get; init; }
    public int ColumnCount { get; init; }
    public bool ShowSocialLinks { get; init; }
    public bool ShowNewsletter { get; init; }

    private FooterOptions() { }

    public static FooterOptions Create(
        FooterTemplate template,
        int columnCount,
        bool showSocialLinks,
        bool showNewsletter)
    {
        if (columnCount < 1 || columnCount > 4)
            throw new ArgumentOutOfRangeException(nameof(columnCount), "Column count must be between 1 and 4.");

        return new FooterOptions
        {
            Template = template,
            ColumnCount = columnCount,
            ShowSocialLinks = showSocialLinks,
            ShowNewsletter = showNewsletter
        };
    }

    public static FooterOptions CreateDefault()
    {
        return new FooterOptions
        {
            Template = FooterTemplate.Standard,
            ColumnCount = 3,
            ShowSocialLinks = true,
            ShowNewsletter = true
        };
    }
}

/// <summary>
/// Value object representing spacing configuration.
/// </summary>
public sealed record SpacingConfiguration
{
    /// <summary>
    /// Container max width in pixels (640-1920).
    /// </summary>
    public int ContainerMaxWidth { get; init; }

    /// <summary>
    /// Section padding in rem (1-8).
    /// </summary>
    public decimal SectionPadding { get; init; }

    /// <summary>
    /// Component gap in rem (0.5-4).
    /// </summary>
    public decimal ComponentGap { get; init; }

    private SpacingConfiguration() { }

    public static SpacingConfiguration Create(int containerMaxWidth, decimal sectionPadding, decimal componentGap)
    {
        if (containerMaxWidth < 640 || containerMaxWidth > 1920)
            throw new ArgumentOutOfRangeException(nameof(containerMaxWidth), "Container max width must be between 640 and 1920 pixels.");

        if (sectionPadding < 1m || sectionPadding > 8m)
            throw new ArgumentOutOfRangeException(nameof(sectionPadding), "Section padding must be between 1 and 8 rem.");

        if (componentGap < 0.5m || componentGap > 4m)
            throw new ArgumentOutOfRangeException(nameof(componentGap), "Component gap must be between 0.5 and 4 rem.");

        return new SpacingConfiguration
        {
            ContainerMaxWidth = containerMaxWidth,
            SectionPadding = sectionPadding,
            ComponentGap = componentGap
        };
    }

    public static SpacingConfiguration CreateDefault()
    {
        return new SpacingConfiguration
        {
            ContainerMaxWidth = 1280,
            SectionPadding = 4m,
            ComponentGap = 1m
        };
    }
}

/// <summary>
/// Value object representing complete layout settings.
/// </summary>
public sealed record LayoutSettings
{
    public HeaderOptions HeaderConfiguration { get; init; } = null!;
    public FooterOptions FooterConfiguration { get; init; } = null!;
    public SpacingConfiguration Spacing { get; init; } = null!;

    private LayoutSettings() { }

    public static LayoutSettings Create(
        HeaderOptions headerConfiguration,
        FooterOptions footerConfiguration,
        SpacingConfiguration spacing)
    {
        ArgumentNullException.ThrowIfNull(headerConfiguration, nameof(headerConfiguration));
        ArgumentNullException.ThrowIfNull(footerConfiguration, nameof(footerConfiguration));
        ArgumentNullException.ThrowIfNull(spacing, nameof(spacing));

        return new LayoutSettings
        {
            HeaderConfiguration = headerConfiguration,
            FooterConfiguration = footerConfiguration,
            Spacing = spacing
        };
    }

    public static LayoutSettings CreateDefault()
    {
        return new LayoutSettings
        {
            HeaderConfiguration = HeaderOptions.CreateDefault(),
            FooterConfiguration = FooterOptions.CreateDefault(),
            Spacing = SpacingConfiguration.CreateDefault()
        };
    }
}
