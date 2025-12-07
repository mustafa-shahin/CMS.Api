using CMS.Domain.Enums;

namespace CMS.Application.Features.LayoutCustomization.DTOs;

public record HeaderOptionsDto
{
    public required HeaderTemplate Template { get; init; }
    public required Placement LogoPlacement { get; init; }
    public required bool ShowSearch { get; init; }
    public required bool StickyHeader { get; init; }
}

public record FooterOptionsDto
{
    public required FooterTemplate Template { get; init; }
    public required int ColumnCount { get; init; }
    public required bool ShowSocialLinks { get; init; }
    public required bool ShowNewsletter { get; init; }
}

public record SpacingConfigurationDto
{
    public required int ContainerMaxWidth { get; init; }
    public required decimal SectionPadding { get; init; }
    public required decimal ComponentGap { get; init; }
}

/// <summary>
/// DTO representing complete layout settings.
/// </summary>
public record LayoutSettingsDto
{
    public required HeaderOptionsDto HeaderConfiguration { get; init; }
    public required FooterOptionsDto FooterConfiguration { get; init; }
    public required SpacingConfigurationDto Spacing { get; init; }
    public DateTime? LastModifiedAt { get; init; }
    public string? LastModifiedBy { get; init; }
}
