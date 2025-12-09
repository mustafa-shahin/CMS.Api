using CMS.Application.Features.LayoutCustomization.DTOs;
using CMS.Application.Features.ThemeCustomization.DTOs;
using CMS.Application.Features.TypographyCustomization.DTOs;
using CMS.Domain.Entities;
using CMS.Domain.Enums;
using CMS.Domain.ValueObjects;

namespace CMS.Application.Mapping;

/// <summary>
/// Mapper for converting between customization domain models and DTOs.
/// </summary>
public static class CustomizationMapper
{
    #region Theme Mapping

    public static ThemeSettingsDto ToDto(this ThemeSettings theme, DateTime? lastModifiedAt = null, string? lastModifiedBy = null)
    {
        return new ThemeSettingsDto
        {
            BrandPalette = theme.BrandPalette.ToDto(),
            NeutralPalette = theme.NeutralPalette.ToDto(),
            SemanticPalette = theme.SemanticPalette.ToDto(),
            LastModifiedAt = lastModifiedAt,
            LastModifiedBy = lastModifiedBy
        };
    }

    public static ColorPaletteDto ToDto(this ColorPalette palette)
    {
        return new ColorPaletteDto
        {
            Primary = palette.Primary.ToDto(),
            Secondary = palette.Secondary.ToDto(),
            Accent = palette.Accent.ToDto()
        };
    }

    public static ColorSchemeDto ToDto(this ColorScheme scheme)
    {
        return new ColorSchemeDto
        {
            Base = scheme.Base,
            Light = scheme.Light,
            Dark = scheme.Dark,
            Contrast = scheme.Contrast
        };
    }

    public static ThemeSettings ToDomain(this ThemeSettingsDto dto)
    {
        return ThemeSettings.Create(
            dto.BrandPalette.ToDomain(),
            dto.NeutralPalette.ToDomain(),
            dto.SemanticPalette.ToDomain()
        );
    }

    public static ColorPalette ToDomain(this ColorPaletteDto dto)
    {
        return ColorPalette.Create(
            dto.Primary.ToDomain(),
            dto.Secondary.ToDomain(),
            dto.Accent.ToDomain()
        );
    }

    public static ColorScheme ToDomain(this ColorSchemeDto dto)
    {
        return ColorScheme.CreateWithVariants(dto.Base, dto.Light, dto.Dark, dto.Contrast);
    }

    #endregion

    #region Typography Mapping

    public static TypographySettingsDto ToDto(this TypographySettings typography, DateTime? lastModifiedAt = null, string? lastModifiedBy = null)
    {
        var textStylesDto = typography.TextStyles.ToDictionary(
            kvp => kvp.Key,
            kvp => kvp.Value.ToDto()
        );

        return new TypographySettingsDto
        {
            PrimaryFontFamily = typography.PrimaryFontFamily,
            SecondaryFontFamily = typography.SecondaryFontFamily,
            MonoFontFamily = typography.MonoFontFamily,
            TextStyles = textStylesDto,
            LastModifiedAt = lastModifiedAt,
            LastModifiedBy = lastModifiedBy
        };
    }

    public static TextStyleDto ToDto(this TextStyle textStyle)
    {
        return new TextStyleDto
        {
            FontFamily = textStyle.FontFamily,
            FontSize = textStyle.FontSize,
            FontWeight = textStyle.FontWeight,
            LineHeight = textStyle.LineHeight,
            LetterSpacing = textStyle.LetterSpacing,
            TextTransform = textStyle.TextTransform
        };
    }

    public static TypographySettings ToDomain(this TypographySettingsDto dto)
    {
        var textStyles = dto.TextStyles.ToDictionary(
            kvp => kvp.Key,
            kvp => kvp.Value.ToDomain()
        );

        return TypographySettings.Create(
            dto.PrimaryFontFamily,
            dto.SecondaryFontFamily,
            dto.MonoFontFamily,
            textStyles
        );
    }

    public static TextStyle ToDomain(this TextStyleDto dto)
    {
        return TextStyle.Create(
            dto.FontFamily,
            dto.FontSize,
            dto.FontWeight,
            dto.LineHeight,
            dto.LetterSpacing,
            dto.TextTransform
        );
    }

    #endregion

    #region Layout Mapping

    public static LayoutSettingsDto ToDto(this LayoutSettings layout, DateTime? lastModifiedAt = null, string? lastModifiedBy = null)
    {
        return new LayoutSettingsDto
        {
            HeaderConfiguration = layout.HeaderConfiguration.ToDto(),
            FooterConfiguration = layout.FooterConfiguration.ToDto(),
            Spacing = layout.Spacing.ToDto(),
            LastModifiedAt = lastModifiedAt,
            LastModifiedBy = lastModifiedBy
        };
    }

    public static HeaderOptionsDto ToDto(this HeaderOptions header)
    {
        return new HeaderOptionsDto
        {
            Template = header.Template,
            LogoPlacement = header.LogoPlacement,
            ShowSearch = header.ShowSearch,
            StickyHeader = header.StickyHeader
        };
    }

    public static FooterOptionsDto ToDto(this FooterOptions footer)
    {
        return new FooterOptionsDto
        {
            Template = footer.Template,
            ColumnCount = footer.ColumnCount,
            ShowSocialLinks = footer.ShowSocialLinks,
            ShowNewsletter = footer.ShowNewsletter
        };
    }

    public static SpacingConfigurationDto ToDto(this SpacingConfiguration spacing)
    {
        return new SpacingConfigurationDto
        {
            ContainerMaxWidth = spacing.ContainerMaxWidth,
            SectionPadding = spacing.SectionPadding,
            ComponentGap = spacing.ComponentGap
        };
    }

    public static LayoutSettings ToDomain(this LayoutSettingsDto dto)
    {
        return LayoutSettings.Create(
            dto.HeaderConfiguration.ToDomain(),
            dto.FooterConfiguration.ToDomain(),
            dto.Spacing.ToDomain()
        );
    }

    public static HeaderOptions ToDomain(this HeaderOptionsDto dto)
    {
        return HeaderOptions.Create(dto.Template, dto.LogoPlacement, dto.ShowSearch, dto.StickyHeader);
    }

    public static FooterOptions ToDomain(this FooterOptionsDto dto)
    {
        return FooterOptions.Create(dto.Template, dto.ColumnCount, dto.ShowSocialLinks, dto.ShowNewsletter);
    }

    public static SpacingConfiguration ToDomain(this SpacingConfigurationDto dto)
    {
        return SpacingConfiguration.Create(dto.ContainerMaxWidth, dto.SectionPadding, dto.ComponentGap);
    }

    #endregion
}
