namespace CMS.Application.Features.ThemeCustomization.DTOs;

/// <summary>
/// DTO representing a color scheme with base color and variants.
/// </summary>
public record ColorSchemeDto
{
    /// <summary>
    /// The base color in hex format (#RRGGBB).
    /// </summary>
    public required string Base { get; init; }

    /// <summary>
    /// Lighter variant of the base color.
    /// </summary>
    public required string Light { get; init; }

    /// <summary>
    /// Darker variant of the base color.
    /// </summary>
    public required string Dark { get; init; }

    /// <summary>
    /// Contrast color for accessibility.
    /// </summary>
    public required string Contrast { get; init; }
}
