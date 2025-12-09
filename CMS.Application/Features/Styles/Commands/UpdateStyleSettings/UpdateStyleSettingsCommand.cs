using System.Text.Json;
using System.Text.RegularExpressions;
using CMS.Application.Common.Exceptions;
using CMS.Application.Common.Interfaces;
using CMS.Application.Common.Validators;
using CMS.Application.Features.Styles.DTOs;
using CMS.Domain.Entities;
using CMS.Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CMS.Application.Features.Styles.Commands.UpdateStyleSettings;

/// <summary>
/// Command to update style settings with primary/secondary color system
/// </summary>
public sealed record UpdateStyleSettingsCommand : IRequest<StyleSettingsDto>
{
    /// <summary>
    /// Primary brand color in hex format (e.g., #3b82f6)
    /// </summary>
    public required string PrimaryColor { get; init; }

    /// <summary>
    /// Secondary/accent color in hex format (e.g., #8b5cf6)
    /// </summary>
    public required string SecondaryColor { get; init; }

    /// <summary>
    /// Navbar background color in hex format (e.g., #2563eb)
    /// </summary>
    public required string NavbarBackground { get; init; }

    /// <summary>
    /// Footer background color in hex format (e.g., #7c3aed)
    /// </summary>
    public required string FooterBackground { get; init; }

    /// <summary>
    /// Body text color in hex format (e.g., #1f2937)
    /// </summary>
    public required string TextColor { get; init; }

    /// <summary>
    /// Heading text color in hex format (e.g., #111827)
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
    /// </summary>
    public string? SuccessColor { get; init; }

    /// <summary>
    /// Warning state color in hex format (e.g., #f59e0b)
    /// </summary>
    public string? WarningColor { get; init; }

    /// <summary>
    /// Error state color in hex format (e.g., #ef4444)
    /// </summary>
    public string? ErrorColor { get; init; }
}

/// <summary>
/// Validator for UpdateStyleSettingsCommand
/// </summary>
public sealed class UpdateStyleSettingsCommandValidator : AbstractValidator<UpdateStyleSettingsCommand>
{
    private static readonly Regex HexColorRegex = new(@"^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$", RegexOptions.Compiled);

    public UpdateStyleSettingsCommandValidator()
    {
        RuleFor(x => x.PrimaryColor)
            .NotEmpty().WithMessage("Primary color is required.")
            .Matches(HexColorRegex).WithMessage("Primary color must be a valid hex color (e.g., #3b82f6).")
            .MaximumLength(7).WithMessage("Primary color must not exceed 7 characters.");

        RuleFor(x => x.SecondaryColor)
            .NotEmpty().WithMessage("Secondary color is required.")
            .Matches(HexColorRegex).WithMessage("Secondary color must be a valid hex color (e.g., #8b5cf6).")
            .MaximumLength(7).WithMessage("Secondary color must not exceed 7 characters.");

        RuleFor(x => x.NavbarBackground)
            .NotEmpty().WithMessage("Navbar background color is required.")
            .Matches(HexColorRegex).WithMessage("Navbar background color must be a valid hex color (e.g., #2563eb).")
            .MaximumLength(7).WithMessage("Navbar background color must not exceed 7 characters.");

        RuleFor(x => x.FooterBackground)
            .NotEmpty().WithMessage("Footer background color is required.")
            .Matches(HexColorRegex).WithMessage("Footer background color must be a valid hex color (e.g., #7c3aed).")
            .MaximumLength(7).WithMessage("Footer background color must not exceed 7 characters.");

        RuleFor(x => x.TextColor)
            .NotEmpty().WithMessage("Text color is required.")
            .Matches(HexColorRegex).WithMessage("Text color must be a valid hex color (e.g., #1f2937).")
            .MaximumLength(7).WithMessage("Text color must not exceed 7 characters.");

        RuleFor(x => x.HeadingColor)
            .NotEmpty().WithMessage("Heading color is required.")
            .Matches(HexColorRegex).WithMessage("Heading color must be a valid hex color (e.g., #111827).")
            .MaximumLength(7).WithMessage("Heading color must not exceed 7 characters.");

        RuleFor(x => x.LinkColor)
            .NotEmpty().WithMessage("Link color is required.")
            .Matches(HexColorRegex).WithMessage("Link color must be a valid hex color (e.g., #3b82f6).")
            .MaximumLength(7).WithMessage("Link color must not exceed 7 characters.");

        RuleFor(x => x.LinkHoverColor)
            .NotEmpty().WithMessage("Link hover color is required.")
            .Matches(HexColorRegex).WithMessage("Link hover color must be a valid hex color (e.g., #2563eb).")
            .MaximumLength(7).WithMessage("Link hover color must not exceed 7 characters.");

        RuleFor(x => x.LinkVisitedColor)
            .NotEmpty().WithMessage("Visited link color is required.")
            .Matches(HexColorRegex).WithMessage("Visited link color must be a valid hex color (e.g., #7c3aed).")
            .MaximumLength(7).WithMessage("Visited link color must not exceed 7 characters.");

        When(x => !string.IsNullOrEmpty(x.SuccessColor), () =>
        {
            RuleFor(x => x.SuccessColor)
                .Matches(HexColorRegex).WithMessage("Success color must be a valid hex color (e.g., #10b981).")
                .MaximumLength(7).WithMessage("Success color must not exceed 7 characters.");
        });

        When(x => !string.IsNullOrEmpty(x.WarningColor), () =>
        {
            RuleFor(x => x.WarningColor)
                .Matches(HexColorRegex).WithMessage("Warning color must be a valid hex color (e.g., #f59e0b).")
                .MaximumLength(7).WithMessage("Warning color must not exceed 7 characters.");
        });

        When(x => !string.IsNullOrEmpty(x.ErrorColor), () =>
        {
            RuleFor(x => x.ErrorColor)
                .Matches(HexColorRegex).WithMessage("Error color must be a valid hex color (e.g., #ef4444).")
                .MaximumLength(7).WithMessage("Error color must not exceed 7 characters.");
        });

        // WCAG Accessibility Validation - Text on Navbar
        RuleFor(x => x)
            .Must(x => ValidateTextOnBackground(x.TextColor, x.NavbarBackground))
            .WithMessage(x => $"Text color on navbar background does not meet WCAG AA standards. " +
                             $"Current contrast ratio: {GetContrastRatio(x.TextColor, x.NavbarBackground):F2}:1 " +
                             $"(minimum required: {ColorContrastValidator.WcagAaMinimumRatio}:1). " +
                             $"Consider using a lighter or darker text color.")
            .WithSeverity(Severity.Warning);

        // WCAG Accessibility Validation - Headings on Navbar
        RuleFor(x => x)
            .Must(x => ValidateTextOnBackground(x.HeadingColor, x.NavbarBackground))
            .WithMessage(x => $"Heading color on navbar background does not meet WCAG AA standards. " +
                             $"Contrast ratio: {GetContrastRatio(x.HeadingColor, x.NavbarBackground):F2}:1")
            .WithSeverity(Severity.Warning);

        // WCAG Accessibility Validation - Text on Footer
        RuleFor(x => x)
            .Must(x => ValidateTextOnBackground(x.TextColor, x.FooterBackground))
            .WithMessage(x => $"Text color on footer background does not meet WCAG AA standards. " +
                             $"Contrast ratio: {GetContrastRatio(x.TextColor, x.FooterBackground):F2}:1")
            .WithSeverity(Severity.Warning);

        // WCAG Accessibility Validation - Link visibility
        RuleFor(x => x)
            .Must(x => ValidateTextOnBackground(x.LinkColor, x.NavbarBackground))
            .WithMessage("Link color should be distinguishable from navbar background for accessibility")
            .WithSeverity(Severity.Warning);
    }

    /// <summary>
    /// Validate text has sufficient contrast on background (WCAG AA: 4.5:1)
    /// </summary>
    private bool ValidateTextOnBackground(string textColor, string backgroundColor)
    {
        try
        {
            return ColorContrastValidator.MeetsWcagAa(textColor, backgroundColor);
        }
        catch
        {
            // If validation fails (invalid color), let other validators handle it
            return true;
        }
    }

    /// <summary>
    /// Get contrast ratio for error messages
    /// </summary>
    private double GetContrastRatio(string foreground, string background)
    {
        try
        {
            return ColorContrastValidator.CalculateContrastRatio(foreground, background);
        }
        catch
        {
            return 0;
        }
    }
}

/// <summary>
/// Handler for UpdateStyleSettingsCommand
/// </summary>
public sealed class UpdateStyleSettingsCommandHandler : IRequestHandler<UpdateStyleSettingsCommand, StyleSettingsDto>
{
    private const string StyleSettingsKey = "app.styles";
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IDateTimeService _dateTime;
    private readonly ILogger<UpdateStyleSettingsCommandHandler> _logger;

    public UpdateStyleSettingsCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser,
        IDateTimeService dateTime,
        ILogger<UpdateStyleSettingsCommandHandler> logger)
    {
        _context = context;
        _currentUser = currentUser;
        _dateTime = dateTime;
        _logger = logger;
    }

    public async Task<StyleSettingsDto> Handle(UpdateStyleSettingsCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation(
                "User {UserId} is updating style settings",
                _currentUser.UserId);

            // Get or create the style settings configuration
            var configuration = await _context.SiteConfigurations
                .FirstOrDefaultAsync(c => c.Key == StyleSettingsKey, cancellationToken);

            var styleData = new
            {
                primaryColor = request.PrimaryColor,
                secondaryColor = request.SecondaryColor,
                navbarBackground = request.NavbarBackground,
                footerBackground = request.FooterBackground,
                textColor = request.TextColor,
                headingColor = request.HeadingColor,
                linkColor = request.LinkColor,
                linkHoverColor = request.LinkHoverColor,
                linkVisitedColor = request.LinkVisitedColor,
                successColor = request.SuccessColor ?? "#10b981",
                warningColor = request.WarningColor ?? "#f59e0b",
                errorColor = request.ErrorColor ?? "#ef4444"
            };

            var jsonValue = JsonDocument.Parse(JsonSerializer.Serialize(styleData));

            if (configuration is null)
            {
                // Create new configuration
                configuration = SiteConfiguration.Create(
                    key: StyleSettingsKey,
                    value: jsonValue,
                    category: ConfigurationCategory.Layout);

                _context.SiteConfigurations.Add(configuration);

                _logger.LogInformation("Created new style settings configuration");
            }
            else
            {
                // Update existing configuration
                configuration.UpdateValue(jsonValue, _currentUser.UserId.Value);

                _logger.LogInformation(
                    "Updated style settings configuration (ID: {ConfigId})",
                    configuration.Id);
            }

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Style settings saved successfully");

            // Get the updated user info for response
            string? updatedByUserName = null;
            if (_currentUser.UserId.HasValue)
            {
                var user = await _context.Users
                    .Where(u => u.Id == _currentUser.UserId.Value)
                    .Select(u => u.FirstName + " " + u.LastName)
                    .FirstOrDefaultAsync(cancellationToken);
                updatedByUserName = user;
            }

            return new StyleSettingsDto
            {
                PrimaryColor = request.PrimaryColor,
                SecondaryColor = request.SecondaryColor,
                NavbarBackground = request.NavbarBackground,
                FooterBackground = request.FooterBackground,
                TextColor = request.TextColor,
                HeadingColor = request.HeadingColor,
                LinkColor = request.LinkColor,
                LinkHoverColor = request.LinkHoverColor,
                LinkVisitedColor = request.LinkVisitedColor,
                SuccessColor = request.SuccessColor ?? "#10b981",
                WarningColor = request.WarningColor ?? "#f59e0b",
                ErrorColor = request.ErrorColor ?? "#ef4444",
                LastUpdatedAt = configuration.LastModifiedAt,
                LastUpdatedBy = updatedByUserName
            };
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(
                ex,
                "Database error while updating style settings for user {UserId}",
                _currentUser.UserId);

            throw new InvalidOperationException(
                "Failed to save style settings. Please try again later.",
                ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Unexpected error while updating style settings for user {UserId}",
                _currentUser.UserId);

            throw new InvalidOperationException(
                "An error occurred while saving style settings. Please contact support if the issue persists.",
                ex);
        }
    }
}
