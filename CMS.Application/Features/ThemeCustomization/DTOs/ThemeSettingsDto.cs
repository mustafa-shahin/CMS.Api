namespace CMS.Application.Features.ThemeCustomization.DTOs;

/// <summary>
/// DTO representing complete theme settings with all color palettes.
/// </summary>
public record ThemeSettingsDto
{
    public required ColorPaletteDto BrandPalette { get; init; }
    public required ColorPaletteDto NeutralPalette { get; init; }
    public required ColorPaletteDto SemanticPalette { get; init; }
    public DateTime? LastModifiedAt { get; init; }
    public string? LastModifiedBy { get; init; }
}
