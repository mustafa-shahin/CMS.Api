namespace CMS.Application.Features.ThemeCustomization.DTOs;

/// <summary>
/// DTO representing a color palette with primary, secondary, and accent colors.
/// </summary>
public record ColorPaletteDto
{
    public required ColorSchemeDto Primary { get; init; }
    public required ColorSchemeDto Secondary { get; init; }
    public required ColorSchemeDto Accent { get; init; }
}
