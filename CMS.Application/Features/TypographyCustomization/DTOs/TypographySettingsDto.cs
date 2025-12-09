using CMS.Domain.Enums;

namespace CMS.Application.Features.TypographyCustomization.DTOs;

/// <summary>
/// DTO representing complete typography settings.
/// </summary>
public record TypographySettingsDto
{
    public required string PrimaryFontFamily { get; init; }
    public required string SecondaryFontFamily { get; init; }
    public required string MonoFontFamily { get; init; }
    public required Dictionary<TextStyleType, TextStyleDto> TextStyles { get; init; }
    public DateTime? LastModifiedAt { get; init; }
    public string? LastModifiedBy { get; init; }
}
