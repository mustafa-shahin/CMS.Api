using CMS.Domain.Enums;

namespace CMS.Application.Features.TypographyCustomization.DTOs;

/// <summary>
/// DTO representing text styling properties.
/// </summary>
public record TextStyleDto
{
    public required string FontFamily { get; init; }
    public required decimal FontSize { get; init; }
    public required int FontWeight { get; init; }
    public required decimal LineHeight { get; init; }
    public decimal? LetterSpacing { get; init; }
    public required TextTransformType TextTransform { get; init; }
}
