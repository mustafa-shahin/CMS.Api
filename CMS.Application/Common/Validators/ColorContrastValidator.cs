using System.Globalization;
using System.Text.RegularExpressions;

namespace CMS.Application.Common.Validators;

/// <summary>
/// Validates color contrast ratios according to WCAG 2.0 standards
/// </summary>
public static class ColorContrastValidator
{
    private static readonly Regex HexColorRegex = new(@"^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$", RegexOptions.Compiled);

    /// <summary>
    /// WCAG 2.0 Level AA contrast ratio requirement for normal text
    /// </summary>
    public const double WcagAaMinimumRatio = 4.5;

    /// <summary>
    /// WCAG 2.0 Level AAA contrast ratio requirement for normal text
    /// </summary>
    public const double WcagAaaMinimumRatio = 7.0;

    /// <summary>
    /// WCAG 2.0 Level AA contrast ratio requirement for large text (18pt+ or 14pt+ bold)
    /// </summary>
    public const double WcagAaLargeTextMinimumRatio = 3.0;

    /// <summary>
    /// WCAG 2.0 Level AAA contrast ratio requirement for large text
    /// </summary>
    public const double WcagAaaLargeTextMinimumRatio = 4.5;

    /// <summary>
    /// Validate if a color string is a valid hex color
    /// </summary>
    public static bool IsValidHexColor(string color)
    {
        return !string.IsNullOrWhiteSpace(color) && HexColorRegex.IsMatch(color);
    }

    /// <summary>
    /// Calculate the contrast ratio between two colors according to WCAG 2.0
    /// </summary>
    /// <param name="foreground">Foreground color in hex format (#RRGGBB or #RGB)</param>
    /// <param name="background">Background color in hex format (#RRGGBB or #RGB)</param>
    /// <returns>Contrast ratio between 1 (no contrast) and 21 (maximum contrast)</returns>
    public static double CalculateContrastRatio(string foreground, string background)
    {
        if (!IsValidHexColor(foreground) || !IsValidHexColor(background))
        {
            throw new ArgumentException("Invalid hex color format. Use #RRGGBB or #RGB");
        }

        var foregroundLuminance = GetRelativeLuminance(foreground);
        var backgroundLuminance = GetRelativeLuminance(background);

        var lighter = Math.Max(foregroundLuminance, backgroundLuminance);
        var darker = Math.Min(foregroundLuminance, backgroundLuminance);

        return (lighter + 0.05) / (darker + 0.05);
    }

    /// <summary>
    /// Check if color combination meets WCAG 2.0 Level AA standard for normal text
    /// </summary>
    public static bool MeetsWcagAa(string foreground, string background)
    {
        var ratio = CalculateContrastRatio(foreground, background);
        return ratio >= WcagAaMinimumRatio;
    }

    /// <summary>
    /// Check if color combination meets WCAG 2.0 Level AAA standard for normal text
    /// </summary>
    public static bool MeetsWcagAaa(string foreground, string background)
    {
        var ratio = CalculateContrastRatio(foreground, background);
        return ratio >= WcagAaaMinimumRatio;
    }

    /// <summary>
    /// Check if color combination meets WCAG 2.0 Level AA standard for large text
    /// </summary>
    public static bool MeetsWcagAaLargeText(string foreground, string background)
    {
        var ratio = CalculateContrastRatio(foreground, background);
        return ratio >= WcagAaLargeTextMinimumRatio;
    }

    /// <summary>
    /// Get detailed WCAG compliance information for a color combination
    /// </summary>
    public static WcagComplianceResult CheckCompliance(string foreground, string background)
    {
        var ratio = CalculateContrastRatio(foreground, background);

        return new WcagComplianceResult
        {
            ContrastRatio = Math.Round(ratio, 2),
            MeetsAA = ratio >= WcagAaMinimumRatio,
            MeetsAAA = ratio >= WcagAaaMinimumRatio,
            MeetsAALargeText = ratio >= WcagAaLargeTextMinimumRatio,
            MeetsAAALargeText = ratio >= WcagAaaLargeTextMinimumRatio,
            Foreground = foreground,
            Background = background
        };
    }

    /// <summary>
    /// Calculate relative luminance according to WCAG 2.0
    /// </summary>
    /// <param name="hexColor">Hex color string (#RRGGBB or #RGB)</param>
    /// <returns>Relative luminance value between 0 (darkest) and 1 (lightest)</returns>
    private static double GetRelativeLuminance(string hexColor)
    {
        var (r, g, b) = HexToRgb(hexColor);

        // Convert to sRGB
        var rsRgb = r / 255.0;
        var gsRgb = g / 255.0;
        var bsRgb = b / 255.0;

        // Apply gamma correction
        var rLinear = rsRgb <= 0.03928 ? rsRgb / 12.92 : Math.Pow((rsRgb + 0.055) / 1.055, 2.4);
        var gLinear = gsRgb <= 0.03928 ? gsRgb / 12.92 : Math.Pow((gsRgb + 0.055) / 1.055, 2.4);
        var bLinear = bsRgb <= 0.03928 ? bsRgb / 12.92 : Math.Pow((bsRgb + 0.055) / 1.055, 2.4);

        // Calculate luminance using WCAG formula
        return 0.2126 * rLinear + 0.7152 * gLinear + 0.0722 * bLinear;
    }

    /// <summary>
    /// Convert hex color to RGB values
    /// </summary>
    private static (int r, int g, int b) HexToRgb(string hex)
    {
        // Remove # if present
        hex = hex.TrimStart('#');

        // Convert 3-digit to 6-digit
        if (hex.Length == 3)
        {
            hex = $"{hex[0]}{hex[0]}{hex[1]}{hex[1]}{hex[2]}{hex[2]}";
        }

        var r = int.Parse(hex.Substring(0, 2), NumberStyles.HexNumber);
        var g = int.Parse(hex.Substring(2, 2), NumberStyles.HexNumber);
        var b = int.Parse(hex.Substring(4, 2), NumberStyles.HexNumber);

        return (r, g, b);
    }

    /// <summary>
    /// Get a contrast color (black or white) that works best on the given background
    /// </summary>
    public static string GetContrastColor(string backgroundColor)
    {
        if (!IsValidHexColor(backgroundColor))
        {
            throw new ArgumentException("Invalid hex color format", nameof(backgroundColor));
        }

        var luminance = GetRelativeLuminance(backgroundColor);
        // Threshold of 0.5 works well for most cases
        return luminance > 0.5 ? "#000000" : "#FFFFFF";
    }
}

/// <summary>
/// WCAG compliance check result
/// </summary>
public class WcagComplianceResult
{
    /// <summary>
    /// Calculated contrast ratio (1-21)
    /// </summary>
    public double ContrastRatio { get; set; }

    /// <summary>
    /// Meets WCAG 2.0 Level AA for normal text (4.5:1)
    /// </summary>
    public bool MeetsAA { get; set; }

    /// <summary>
    /// Meets WCAG 2.0 Level AAA for normal text (7:1)
    /// </summary>
    public bool MeetsAAA { get; set; }

    /// <summary>
    /// Meets WCAG 2.0 Level AA for large text (3:1)
    /// </summary>
    public bool MeetsAALargeText { get; set; }

    /// <summary>
    /// Meets WCAG 2.0 Level AAA for large text (4.5:1)
    /// </summary>
    public bool MeetsAAALargeText { get; set; }

    /// <summary>
    /// Foreground color that was tested
    /// </summary>
    public string Foreground { get; set; } = string.Empty;

    /// <summary>
    /// Background color that was tested
    /// </summary>
    public string Background { get; set; } = string.Empty;

    /// <summary>
    /// Get a human-readable description of compliance level
    /// </summary>
    public string GetComplianceDescription()
    {
        if (MeetsAAA)
            return "Excellent contrast (WCAG AAA)";
        if (MeetsAA)
            return "Good contrast (WCAG AA)";
        if (MeetsAALargeText)
            return "Acceptable for large text only (WCAG AA Large)";

        return $"Poor contrast ({ContrastRatio:F2}:1) - Does not meet WCAG standards";
    }
}
