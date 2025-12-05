using System.Text.Json;
using CMS.Application.Common.Interfaces;
using CMS.Application.Features.Styles.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CMS.Application.Features.Styles.Queries.GetStyleSettings;

/// <summary>
/// Query to get current style settings
/// </summary>
public sealed record GetStyleSettingsQuery : IRequest<StyleSettingsDto>;

/// <summary>
/// Handler for GetStyleSettingsQuery
/// </summary>
public sealed class GetStyleSettingsQueryHandler : IRequestHandler<GetStyleSettingsQuery, StyleSettingsDto>
{
    private const string StyleSettingsKey = "app.styles";
    private const string DefaultPrimaryColor = "#3b82f6";  // Blue
    private const string DefaultSecondaryColor = "#8b5cf6"; // Purple
    private const string DefaultNavbarBackground = "#1e40af"; // Darker blue (blue-800)
    private const string DefaultFooterBackground = "#6d28d9"; // Darker purple (purple-700)
    private const string DefaultTextColor = "#1f2937";      // Gray-800
    private const string DefaultHeadingColor = "#111827";   // Gray-900
    private const string DefaultLinkColor = "#3b82f6";      // Blue-500
    private const string DefaultLinkHoverColor = "#2563eb"; // Blue-600
    private const string DefaultLinkVisitedColor = "#7c3aed"; // Purple-600
    private const string DefaultSuccessColor = "#10b981";   // Green
    private const string DefaultWarningColor = "#f59e0b";   // Amber
    private const string DefaultErrorColor = "#ef4444";     // Red

    private readonly IApplicationDbContext _context;
    private readonly ILogger<GetStyleSettingsQueryHandler> _logger;

    public GetStyleSettingsQueryHandler(
        IApplicationDbContext context,
        ILogger<GetStyleSettingsQueryHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<StyleSettingsDto> Handle(GetStyleSettingsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving style settings");

        var configuration = await _context.SiteConfigurations
            .Include(c => c.UpdatedByUser)
            .FirstOrDefaultAsync(c => c.Key == StyleSettingsKey, cancellationToken);

        if (configuration is null)
        {
            _logger.LogInformation("No style settings found, returning defaults");

            return new StyleSettingsDto
            {
                PrimaryColor = DefaultPrimaryColor,
                SecondaryColor = DefaultSecondaryColor,
                NavbarBackground = DefaultNavbarBackground,
                FooterBackground = DefaultFooterBackground,
                TextColor = DefaultTextColor,
                HeadingColor = DefaultHeadingColor,
                LinkColor = DefaultLinkColor,
                LinkHoverColor = DefaultLinkHoverColor,
                LinkVisitedColor = DefaultLinkVisitedColor,
                SuccessColor = DefaultSuccessColor,
                WarningColor = DefaultWarningColor,
                ErrorColor = DefaultErrorColor,
                LastUpdatedAt = null,
                LastUpdatedBy = null
            };
        }

        // Parse JSON value
        string primaryColor = DefaultPrimaryColor;
        string secondaryColor = DefaultSecondaryColor;
        string navbarBackground = DefaultNavbarBackground;
        string footerBackground = DefaultFooterBackground;
        string textColor = DefaultTextColor;
        string headingColor = DefaultHeadingColor;
        string linkColor = DefaultLinkColor;
        string linkHoverColor = DefaultLinkHoverColor;
        string linkVisitedColor = DefaultLinkVisitedColor;
        string successColor = DefaultSuccessColor;
        string warningColor = DefaultWarningColor;
        string errorColor = DefaultErrorColor;

        try
        {
            // JsonDocument does not have a Clone method; use the existing instance directly
            var root = configuration.Value.RootElement;

            if (root.TryGetProperty("primaryColor", out var primaryProp))
            {
                primaryColor = primaryProp.GetString() ?? DefaultPrimaryColor;
            }

            if (root.TryGetProperty("secondaryColor", out var secondaryProp))
            {
                secondaryColor = secondaryProp.GetString() ?? DefaultSecondaryColor;
            }

            if (root.TryGetProperty("navbarBackground", out var navbarProp))
            {
                navbarBackground = navbarProp.GetString() ?? DefaultNavbarBackground;
            }

            if (root.TryGetProperty("footerBackground", out var footerProp))
            {
                footerBackground = footerProp.GetString() ?? DefaultFooterBackground;
            }

            if (root.TryGetProperty("textColor", out var textProp))
            {
                textColor = textProp.GetString() ?? DefaultTextColor;
            }

            if (root.TryGetProperty("headingColor", out var headingProp))
            {
                headingColor = headingProp.GetString() ?? DefaultHeadingColor;
            }

            if (root.TryGetProperty("linkColor", out var linkProp))
            {
                linkColor = linkProp.GetString() ?? DefaultLinkColor;
            }

            if (root.TryGetProperty("linkHoverColor", out var linkHoverProp))
            {
                linkHoverColor = linkHoverProp.GetString() ?? DefaultLinkHoverColor;
            }

            if (root.TryGetProperty("linkVisitedColor", out var linkVisitedProp))
            {
                linkVisitedColor = linkVisitedProp.GetString() ?? DefaultLinkVisitedColor;
            }

            if (root.TryGetProperty("successColor", out var successProp))
            {
                successColor = successProp.GetString() ?? DefaultSuccessColor;
            }

            if (root.TryGetProperty("warningColor", out var warningProp))
            {
                warningColor = warningProp.GetString() ?? DefaultWarningColor;
            }

            if (root.TryGetProperty("errorColor", out var errorProp))
            {
                errorColor = errorProp.GetString() ?? DefaultErrorColor;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to parse style settings JSON, using defaults");
        }

        string? updatedByUserName = configuration.UpdatedByUser is not null
            ? $"{configuration.UpdatedByUser.FirstName} {configuration.UpdatedByUser.LastName}"
            : null;

        _logger.LogInformation(
            "Retrieved style settings: PrimaryColor={PrimaryColor}, SecondaryColor={SecondaryColor}",
            primaryColor, secondaryColor);

        return new StyleSettingsDto
        {
            PrimaryColor = primaryColor,
            SecondaryColor = secondaryColor,
            NavbarBackground = navbarBackground,
            FooterBackground = footerBackground,
            TextColor = textColor,
            HeadingColor = headingColor,
            LinkColor = linkColor,
            LinkHoverColor = linkHoverColor,
            LinkVisitedColor = linkVisitedColor,
            SuccessColor = successColor,
            WarningColor = warningColor,
            ErrorColor = errorColor,
            LastUpdatedAt = configuration.LastModifiedAt,
            LastUpdatedBy = updatedByUserName
        };
    }
}
