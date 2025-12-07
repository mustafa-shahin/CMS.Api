using CMS.Domain.Enums;

namespace CMS.Domain.ValueObjects;

/// <summary>
/// Value object representing complete typography settings.
/// </summary>
public sealed record TypographySettings
{
    /// <summary>
    /// Primary font family for headings.
    /// </summary>
    public string PrimaryFontFamily { get; init; } = null!;

    /// <summary>
    /// Secondary font family for body text.
    /// </summary>
    public string SecondaryFontFamily { get; init; } = null!;

    /// <summary>
    /// Monospace font family for code.
    /// </summary>
    public string MonoFontFamily { get; init; } = null!;

    /// <summary>
    /// Dictionary of text styles for different text types.
    /// </summary>
    public Dictionary<TextStyleType, TextStyle> TextStyles { get; init; } = null!;

    /// <summary>
    /// Private constructor for EF Core and deserialization.
    /// </summary>
    public TypographySettings() { }

    /// <summary>
    /// Creates new typography settings.
    /// </summary>
    public static TypographySettings Create(
        string primaryFontFamily,
        string secondaryFontFamily,
        string monoFontFamily,
        Dictionary<TextStyleType, TextStyle> textStyles)
    {
        if (string.IsNullOrWhiteSpace(primaryFontFamily))
            throw new ArgumentException("Primary font family cannot be null or empty.", nameof(primaryFontFamily));

        if (string.IsNullOrWhiteSpace(secondaryFontFamily))
            throw new ArgumentException("Secondary font family cannot be null or empty.", nameof(secondaryFontFamily));

        if (string.IsNullOrWhiteSpace(monoFontFamily))
            throw new ArgumentException("Mono font family cannot be null or empty.", nameof(monoFontFamily));

        ArgumentNullException.ThrowIfNull(textStyles, nameof(textStyles));

        return new TypographySettings
        {
            PrimaryFontFamily = primaryFontFamily.Trim(),
            SecondaryFontFamily = secondaryFontFamily.Trim(),
            MonoFontFamily = monoFontFamily.Trim(),
            TextStyles = textStyles
        };
    }

    /// <summary>
    /// Creates default typography settings with standard text styles.
    /// </summary>
    public static TypographySettings CreateDefault()
    {
        var textStyles = new Dictionary<TextStyleType, TextStyle>();

        // Add all default text styles
        foreach (TextStyleType styleType in Enum.GetValues(typeof(TextStyleType)))
        {
            textStyles[styleType] = TextStyle.CreateDefault(styleType);
        }

        return new TypographySettings
        {
            PrimaryFontFamily = "Inter",
            SecondaryFontFamily = "Inter",
            MonoFontFamily = "JetBrains Mono",
            TextStyles = textStyles
        };
    }

    /// <summary>
    /// Gets a text style by type.
    /// </summary>
    public TextStyle GetTextStyle(TextStyleType type)
    {
        return TextStyles.TryGetValue(type, out var style) ? style : TextStyle.CreateDefault(type);
    }
}
