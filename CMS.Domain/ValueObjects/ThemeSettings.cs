namespace CMS.Domain.ValueObjects;

/// <summary>
/// Value object representing complete theme settings including all color palettes.
/// </summary>
public sealed record ThemeSettings
{
    /// <summary>
    /// Brand colors (primary, secondary, accent for branding).
    /// </summary>
    public ColorPalette BrandPalette { get; init; } = null!;

    /// <summary>
    /// Neutral colors (light, medium, dark for UI backgrounds and borders).
    /// </summary>
    public ColorPalette NeutralPalette { get; init; } = null!;

    /// <summary>
    /// Semantic colors (success, warning, error for feedback).
    /// </summary>
    public ColorPalette SemanticPalette { get; init; } = null!;

    /// <summary>
    /// Private constructor for EF Core and deserialization.
    /// </summary>
    public ThemeSettings() { }

    /// <summary>
    /// Creates new theme settings with all color palettes.
    /// </summary>
    public static ThemeSettings Create(ColorPalette brandPalette, ColorPalette neutralPalette, ColorPalette semanticPalette)
    {
        ArgumentNullException.ThrowIfNull(brandPalette, nameof(brandPalette));
        ArgumentNullException.ThrowIfNull(neutralPalette, nameof(neutralPalette));
        ArgumentNullException.ThrowIfNull(semanticPalette, nameof(semanticPalette));

        return new ThemeSettings
        {
            BrandPalette = brandPalette,
            NeutralPalette = neutralPalette,
            SemanticPalette = semanticPalette
        };
    }

    /// <summary>
    /// Creates default theme settings with standard colors.
    /// </summary>
    public static ThemeSettings CreateDefault()
    {
        return new ThemeSettings
        {
            BrandPalette = ColorPalette.CreateFromBase("#3B82F6", "#8B5CF6", "#EC4899"),  // Blue, Purple, Pink
            NeutralPalette = ColorPalette.CreateFromBase("#F3F4F6", "#9CA3AF", "#1F2937"), // Light gray, Medium gray, Dark gray
            SemanticPalette = ColorPalette.CreateFromBase("#10B981", "#F59E0B", "#EF4444") // Green, Amber, Red
        };
    }
}
