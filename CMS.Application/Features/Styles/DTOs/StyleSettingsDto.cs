namespace CMS.Application.Features.Styles.DTOs;

/// <summary>
/// DTO for application style settings with primary/secondary color system
/// </summary>
public sealed record StyleSettingsDto
{
    /// <summary>
    /// Primary brand color in hex format (e.g., #3b82f6)
    /// Used for: navbar, primary buttons, links, active states
    /// </summary>
    public required string PrimaryColor { get; init; }

    /// <summary>
    /// Secondary/accent color in hex format (e.g., #8b5cf6)
    /// Used for: footer, secondary buttons, highlights, accents
    /// </summary>
    public required string SecondaryColor { get; init; }

    /// <summary>
    /// Navbar background color in hex format (e.g., #2563eb)
    /// Derived from primary color palette
    /// </summary>
    public required string NavbarBackground { get; init; }

    /// <summary>
    /// Footer background color in hex format (e.g., #7c3aed)
    /// Derived from secondary color palette
    /// </summary>
    public required string FooterBackground { get; init; }

    /// <summary>
    /// Body text color in hex format (e.g., #1f2937)
    /// </summary>
    public required string TextColor { get; init; }

    /// <summary>
    /// Heading text color for H1, H2, H3, etc. in hex format (e.g., #111827)
    /// </summary>
    public required string HeadingColor { get; init; }

    /// <summary>
    /// Link color in hex format (e.g., #3b82f6)
    /// </summary>
    public required string LinkColor { get; init; }

    /// <summary>
    /// Link hover color in hex format (e.g., #2563eb)
    /// </summary>
    public required string LinkHoverColor { get; init; }

    /// <summary>
    /// Visited link color in hex format (e.g., #7c3aed)
    /// </summary>
    public required string LinkVisitedColor { get; init; }

    /// <summary>
    /// Success state color in hex format (e.g., #10b981)
    /// Used for: success messages, positive actions
    /// </summary>
    public string? SuccessColor { get; init; }

    /// <summary>
    /// Warning state color in hex format (e.g., #f59e0b)
    /// Used for: warning messages, caution indicators
    /// </summary>
    public string? WarningColor { get; init; }

    /// <summary>
    /// Error state color in hex format (e.g., #ef4444)
    /// Used for: error messages, destructive actions
    /// </summary>
    public string? ErrorColor { get; init; }

    /// <summary>
    /// Timestamp of last update
    /// </summary>
    public DateTime? LastUpdatedAt { get; init; }

    /// <summary>
    /// User who last updated the settings
    /// </summary>
    public string? LastUpdatedBy { get; init; }
}
