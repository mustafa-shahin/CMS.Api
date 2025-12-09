using System.Text.Json;
using CMS.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace CMS.Infrastructure.Persistence.Converters;

/// <summary>
/// Value converters for serializing value objects to/from JSON.
/// </summary>
public static class ValueObjectJsonConverters
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public static ValueConverter<ThemeSettings, string> ThemeSettingsConverter =>
        new(
            v => JsonSerializer.Serialize(v, JsonOptions),
            v => JsonSerializer.Deserialize<ThemeSettings>(v, JsonOptions)!
        );

    public static ValueConverter<TypographySettings, string> TypographySettingsConverter =>
        new(
            v => JsonSerializer.Serialize(v, JsonOptions),
            v => JsonSerializer.Deserialize<TypographySettings>(v, JsonOptions)!
        );

    public static ValueConverter<LayoutSettings, string> LayoutSettingsConverter =>
        new(
            v => JsonSerializer.Serialize(v, JsonOptions),
            v => JsonSerializer.Deserialize<LayoutSettings>(v, JsonOptions)!
        );

    public static ValueConverter<BrandingSettings, string> BrandingSettingsConverter =>
        new(
            v => JsonSerializer.Serialize(v, JsonOptions),
            v => JsonSerializer.Deserialize<BrandingSettings>(v, JsonOptions)!
        );
}
