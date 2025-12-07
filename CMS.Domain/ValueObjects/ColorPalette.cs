namespace CMS.Domain.ValueObjects;

/// <summary>
/// Value object representing a color palette with primary, secondary, and accent colors.
/// Used for brand, neutral, and semantic color groups.
/// </summary>
public sealed record ColorPalette
{
    /// <summary>
    /// Primary color scheme in this palette.
    /// </summary>
    public ColorScheme Primary { get; init; } = null!;

    /// <summary>
    /// Secondary color scheme in this palette.
    /// </summary>
    public ColorScheme Secondary { get; init; } = null!;

    /// <summary>
    /// Accent color scheme in this palette.
    /// </summary>
    public ColorScheme Accent { get; init; } = null!;

    /// <summary>
    /// Private constructor for EF Core and deserialization.
    /// </summary>
    private ColorPalette() { }

    /// <summary>
    /// Creates a new color palette with the specified color schemes.
    /// </summary>
    public static ColorPalette Create(ColorScheme primary, ColorScheme secondary, ColorScheme accent)
    {
        ArgumentNullException.ThrowIfNull(primary, nameof(primary));
        ArgumentNullException.ThrowIfNull(secondary, nameof(secondary));
        ArgumentNullException.ThrowIfNull(accent, nameof(accent));

        return new ColorPalette
        {
            Primary = primary,
            Secondary = secondary,
            Accent = accent
        };
    }

    /// <summary>
    /// Creates a color palette from base colors (variants are auto-generated).
    /// </summary>
    public static ColorPalette CreateFromBase(string primaryBase, string secondaryBase, string accentBase)
    {
        return new ColorPalette
        {
            Primary = ColorScheme.Create(primaryBase),
            Secondary = ColorScheme.Create(secondaryBase),
            Accent = ColorScheme.Create(accentBase)
        };
    }
}
