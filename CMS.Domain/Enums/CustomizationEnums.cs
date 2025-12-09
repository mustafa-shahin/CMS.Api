namespace CMS.Domain.Enums;

/// <summary>
/// Defines the types of text styles available for customization.
/// </summary>
public enum TextStyleType
{
    // Headings
    Heading1 = 0,
    Heading2 = 1,
    Heading3 = 2,
    Heading4 = 3,
    Heading5 = 4,
    Heading6 = 5,

    // Body text
    BodyLarge = 10,
    BodyMedium = 11,
    BodySmall = 12,

    // Special text
    Caption = 20,
    Overline = 21,
    ButtonText = 22,
    LinkText = 23
}

/// <summary>
/// Text transformation styles.
/// </summary>
public enum TextTransformType
{
    None = 0,
    Uppercase = 1,
    Lowercase = 2,
    Capitalize = 3
}

/// <summary>
/// Header template variations.
/// </summary>
public enum HeaderTemplate
{
    Minimal = 0,
    Standard = 1,
    Full = 2
}

/// <summary>
/// Footer template variations.
/// </summary>
public enum FooterTemplate
{
    Minimal = 0,
    Standard = 1,
    Full = 2
}

/// <summary>
/// Element placement options.
/// </summary>
public enum Placement
{
    Left = 0,
    Center = 1,
    Right = 2
}
