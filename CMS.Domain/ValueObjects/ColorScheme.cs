using System.Text.RegularExpressions;

namespace CMS.Domain.ValueObjects;

/// <summary>
/// Value object representing a color scheme with base color and its variants.
/// Ensures color values are valid hex format and automatically calculates variants.
/// </summary>
public sealed partial record ColorScheme
{
    public static readonly Regex HexColorRegex = GenerateHexColorRegex();

    /// <summary>
    /// The base color in hexadecimal format (#RGB or #RRGGBB).
    /// </summary>
    public string Base { get; init; } = null!;

    /// <summary>
    /// Lighter variant of the base color (approximately 20% lighter).
    /// </summary>
    public string Light { get; init; } = null!;

    /// <summary>
    /// Darker variant of the base color (approximately 20% darker).
    /// </summary>
    public string Dark { get; init; } = null!;

    /// <summary>
    /// Contrast color for text readability (either black or white).
    /// </summary>
    public string Contrast { get; init; } = null!;

    /// <summary>
    /// Private constructor for EF Core and deserialization.
    /// </summary>
    public ColorScheme() { }

    /// <summary>
    /// Creates a new color scheme with the specified base color.
    /// Automatically generates light, dark, and contrast variants.
    /// </summary>
    /// <param name="baseColor">The base color in hex format.</param>
    /// <returns>A new ColorScheme with all variants.</returns>
    /// <exception cref="ArgumentException">Thrown when baseColor is not a valid hex color.</exception>
    public static ColorScheme Create(string baseColor)
    {
        if (string.IsNullOrWhiteSpace(baseColor))
            throw new ArgumentException("Base color cannot be null or empty.", nameof(baseColor));

        if (!IsValidHexColor(baseColor))
            throw new ArgumentException($"Invalid hex color format: {baseColor}. Expected format: #RGB or #RRGGBB.", nameof(baseColor));

        var normalizedBase = NormalizeHexColor(baseColor);

        return new ColorScheme
        {
            Base = normalizedBase,
            Light = GenerateLighterColor(normalizedBase, 0.2),
            Dark = GenerateDarkerColor(normalizedBase, 0.2),
            Contrast = GenerateContrastColor(normalizedBase)
        };
    }

    /// <summary>
    /// Creates a color scheme with manually specified variants.
    /// Useful for importing or custom configurations.
    /// </summary>
    public static ColorScheme CreateWithVariants(string baseColor, string lightColor, string darkColor, string contrastColor)
    {
        if (!IsValidHexColor(baseColor) || !IsValidHexColor(lightColor) ||
            !IsValidHexColor(darkColor) || !IsValidHexColor(contrastColor))
            throw new ArgumentException("All colors must be valid hex format.");

        return new ColorScheme
        {
            Base = NormalizeHexColor(baseColor),
            Light = NormalizeHexColor(lightColor),
            Dark = NormalizeHexColor(darkColor),
            Contrast = NormalizeHexColor(contrastColor)
        };
    }

    /// <summary>
    /// Validates if a string is a valid hex color format.
    /// </summary>
    public static bool IsValidHexColor(string? color)
    {
        if (string.IsNullOrWhiteSpace(color))
            return false;

        return HexColorRegex.IsMatch(color);
    }

    /// <summary>
    /// Normalizes hex color to #RRGGBB format (expands #RGB to #RRGGBB).
    /// </summary>
    private static string NormalizeHexColor(string hex)
    {
        hex = hex.Trim().ToUpperInvariant();

        // If short form (#RGB), expand to #RRGGBB
        if (hex.Length == 4)
        {
            return $"#{hex[1]}{hex[1]}{hex[2]}{hex[2]}{hex[3]}{hex[3]}";
        }

        return hex;
    }

    /// <summary>
    /// Generates a lighter color by interpolating with white.
    /// </summary>
    private static string GenerateLighterColor(string hex, double factor)
    {
        return InterpolateColor(hex, "#FFFFFF", factor);
    }

    /// <summary>
    /// Generates a darker color by interpolating with black.
    /// </summary>
    private static string GenerateDarkerColor(string hex, double factor)
    {
        return InterpolateColor(hex, "#000000", factor);
    }

    /// <summary>
    /// Generates a contrast color (black or white) based on luminance.
    /// </summary>
    private static string GenerateContrastColor(string hex)
    {
        var rgb = HexToRgb(hex);
        // Calculate relative luminance using WCAG formula
        var luminance = (0.2126 * rgb.r + 0.7152 * rgb.g + 0.0722 * rgb.b) / 255.0;

        // Return white for dark colors, black for light colors
        return luminance > 0.5 ? "#000000" : "#FFFFFF";
    }

    /// <summary>
    /// Interpolates between two colors.
    /// </summary>
    private static string InterpolateColor(string color1, string color2, double factor)
    {
        var rgb1 = HexToRgb(color1);
        var rgb2 = HexToRgb(color2);

        var r = (int)Math.Round(rgb1.r + factor * (rgb2.r - rgb1.r));
        var g = (int)Math.Round(rgb1.g + factor * (rgb2.g - rgb1.g));
        var b = (int)Math.Round(rgb1.b + factor * (rgb2.b - rgb1.b));

        return RgbToHex(r, g, b);
    }

    /// <summary>
    /// Converts hex color to RGB values.
    /// </summary>
    private static (int r, int g, int b) HexToRgb(string hex)
    {
        hex = hex.Replace("#", "");
        return (
            Convert.ToInt32(hex.Substring(0, 2), 16),
            Convert.ToInt32(hex.Substring(2, 2), 16),
            Convert.ToInt32(hex.Substring(4, 2), 16)
        );
    }

    /// <summary>
    /// Converts RGB values to hex color.
    /// </summary>
    private static string RgbToHex(int r, int g, int b)
    {
        r = Math.Clamp(r, 0, 255);
        g = Math.Clamp(g, 0, 255);
        b = Math.Clamp(b, 0, 255);

        return $"#{r:X2}{g:X2}{b:X2}";
    }

    [GeneratedRegex(@"^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$")]
    public static partial Regex GenerateHexColorRegex();
}
