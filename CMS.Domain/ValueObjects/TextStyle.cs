using CMS.Domain.Enums;

namespace CMS.Domain.ValueObjects;

/// <summary>
/// Value object representing text styling properties.
/// </summary>
public sealed record TextStyle
{
    /// <summary>
    /// Font family name (e.g., 'Inter', 'Roboto').
    /// </summary>
    public string FontFamily { get; init; } = null!;

    /// <summary>
    /// Font size in rem units (0.5 to 10).
    /// </summary>
    public decimal FontSize { get; init; }

    /// <summary>
    /// Font weight (100-900 in increments of 100).
    /// </summary>
    public int FontWeight { get; init; }

    /// <summary>
    /// Line height (unitless multiplier, e.g., 1.5).
    /// </summary>
    public decimal LineHeight { get; init; }

    /// <summary>
    /// Letter spacing in em units (optional).
    /// </summary>
    public decimal? LetterSpacing { get; init; }

    /// <summary>
    /// Text transformation (none, uppercase, lowercase, capitalize).
    /// </summary>
    public TextTransformType TextTransform { get; init; }

    /// <summary>
    /// Private constructor for EF Core and deserialization.
    /// </summary>
    private TextStyle() { }

    /// <summary>
    /// Creates a new text style with validation.
    /// </summary>
    public static TextStyle Create(
        string fontFamily,
        decimal fontSize,
        int fontWeight,
        decimal lineHeight,
        decimal? letterSpacing = null,
        TextTransformType textTransform = TextTransformType.None)
    {
        // Validation
        if (string.IsNullOrWhiteSpace(fontFamily))
            throw new ArgumentException("Font family cannot be null or empty.", nameof(fontFamily));

        if (fontSize < 0.5m || fontSize > 10m)
            throw new ArgumentOutOfRangeException(nameof(fontSize), "Font size must be between 0.5 and 10 rem.");

        if (fontWeight < 100 || fontWeight > 900 || fontWeight % 100 != 0)
            throw new ArgumentOutOfRangeException(nameof(fontWeight), "Font weight must be between 100 and 900 in increments of 100.");

        if (lineHeight < 0.8m || lineHeight > 3m)
            throw new ArgumentOutOfRangeException(nameof(lineHeight), "Line height must be between 0.8 and 3.");

        if (letterSpacing.HasValue && (letterSpacing.Value < -0.1m || letterSpacing.Value > 0.5m))
            throw new ArgumentOutOfRangeException(nameof(letterSpacing), "Letter spacing must be between -0.1 and 0.5 em.");

        return new TextStyle
        {
            FontFamily = fontFamily.Trim(),
            FontSize = fontSize,
            FontWeight = fontWeight,
            LineHeight = lineHeight,
            LetterSpacing = letterSpacing,
            TextTransform = textTransform
        };
    }

    /// <summary>
    /// Creates a text style with default values.
    /// </summary>
    public static TextStyle CreateDefault(TextStyleType type)
    {
        return type switch
        {
            TextStyleType.Heading1 => Create("Inter", 2.5m, 700, 1.2m),
            TextStyleType.Heading2 => Create("Inter", 2m, 700, 1.3m),
            TextStyleType.Heading3 => Create("Inter", 1.75m, 600, 1.3m),
            TextStyleType.Heading4 => Create("Inter", 1.5m, 600, 1.4m),
            TextStyleType.Heading5 => Create("Inter", 1.25m, 600, 1.4m),
            TextStyleType.Heading6 => Create("Inter", 1m, 600, 1.5m),
            TextStyleType.BodyLarge => Create("Inter", 1.125m, 400, 1.6m),
            TextStyleType.BodyMedium => Create("Inter", 1m, 400, 1.6m),
            TextStyleType.BodySmall => Create("Inter", 0.875m, 400, 1.5m),
            TextStyleType.Caption => Create("Inter", 0.75m, 400, 1.4m),
            TextStyleType.Overline => Create("Inter", 0.75m, 500, 1.2m, 0.1m, TextTransformType.Uppercase),
            TextStyleType.ButtonText => Create("Inter", 1m, 500, 1.5m, null, TextTransformType.None),
            TextStyleType.LinkText => Create("Inter", 1m, 400, 1.5m),
            _ => Create("Inter", 1m, 400, 1.5m)
        };
    }
}
