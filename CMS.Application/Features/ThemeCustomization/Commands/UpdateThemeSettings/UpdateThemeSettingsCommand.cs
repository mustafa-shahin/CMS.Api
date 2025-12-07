using CMS.Application.Common.Interfaces;
using CMS.Application.Features.ThemeCustomization.DTOs;
using CMS.Application.Mapping;
using CMS.Domain.Entities;
using CMS.Domain.ValueObjects;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CMS.Application.Features.ThemeCustomization.Commands.UpdateThemeSettings;

/// <summary>
/// Command to update theme settings including all color palettes.
/// </summary>
public sealed record UpdateThemeSettingsCommand : IRequest<ThemeSettingsDto>
{
    public required ThemeSettingsDto ThemeSettings { get; init; }
}

/// <summary>
/// Validator for UpdateThemeSettingsCommand
/// </summary>
public sealed class UpdateThemeSettingsCommandValidator : AbstractValidator<UpdateThemeSettingsCommand>
{
    public UpdateThemeSettingsCommandValidator()
    {
        RuleFor(x => x.ThemeSettings)
            .NotNull().WithMessage("Theme settings are required.");

        RuleFor(x => x.ThemeSettings.BrandPalette)
            .NotNull().WithMessage("Brand palette is required.");

        RuleFor(x => x.ThemeSettings.NeutralPalette)
            .NotNull().WithMessage("Neutral palette is required.");

        RuleFor(x => x.ThemeSettings.SemanticPalette)
            .NotNull().WithMessage("Semantic palette is required.");

        // Validate all color schemes
        RuleForEach(x => new[]
        {
            x.ThemeSettings.BrandPalette != null ? x.ThemeSettings.BrandPalette.Primary : null,
            x.ThemeSettings.BrandPalette != null ? x.ThemeSettings.BrandPalette.Secondary : null,
            x.ThemeSettings.BrandPalette != null ? x.ThemeSettings.BrandPalette.Accent : null,
            x.ThemeSettings.NeutralPalette != null ? x.ThemeSettings.NeutralPalette.Primary : null,
            x.ThemeSettings.NeutralPalette != null ? x.ThemeSettings.NeutralPalette.Secondary : null,
            x.ThemeSettings.NeutralPalette != null ? x.ThemeSettings.NeutralPalette.Accent : null,
            x.ThemeSettings.SemanticPalette != null ? x.ThemeSettings.SemanticPalette.Primary : null,
            x.ThemeSettings.SemanticPalette != null ? x.ThemeSettings.SemanticPalette.Secondary : null,
            x.ThemeSettings.SemanticPalette != null ? x.ThemeSettings.SemanticPalette.Accent : null
        })
        .Must(scheme => scheme == null || IsValidColorScheme(scheme))
        .WithMessage("All color values must be valid hex colors.");
    }

    private static bool IsValidColorScheme(ColorSchemeDto scheme)
    {
        return ColorScheme.IsValidHexColor(scheme.Base) &&
               ColorScheme.IsValidHexColor(scheme.Light) &&
               ColorScheme.IsValidHexColor(scheme.Dark) &&
               ColorScheme.IsValidHexColor(scheme.Contrast);
    }
}

/// <summary>
/// Handler for UpdateThemeSettingsCommand
/// </summary>
public sealed class UpdateThemeSettingsCommandHandler : IRequestHandler<UpdateThemeSettingsCommand, ThemeSettingsDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<UpdateThemeSettingsCommandHandler> _logger;

    public UpdateThemeSettingsCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser,
        ILogger<UpdateThemeSettingsCommandHandler> logger)
    {
        _context = context;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<ThemeSettingsDto> Handle(UpdateThemeSettingsCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating theme settings for user {UserId}", _currentUser.UserId);

        // Get or create active customization settings
        var settings = await _context.CustomizationSettings
            .Where(s => s.IsActive)
            .FirstOrDefaultAsync(cancellationToken);

        var themeSettings = request.ThemeSettings.ToDomain();

        if (settings is null)
        {
            // Create new settings with default typography and layout
            settings = CustomizationSettings.Create(
                themeSettings,
                TypographySettings.CreateDefault(),
                LayoutSettings.CreateDefault()
            );

            _context.CustomizationSettings.Add(settings);
            _logger.LogInformation("Created new customization settings");
        }
        else
        {
            // Update theme in existing settings
            settings.UpdateTheme(themeSettings, _currentUser.UserId.Value);
            _logger.LogInformation("Updated theme in existing customization settings with ID {SettingsId}", settings.Id);
        }

        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Theme settings saved successfully");

        // Get updated user info
        string? updatedByUserName = null;
        if (settings.LastModifiedBy.HasValue)
        {
            var user = await _context.Users
                .Where(u => u.Id == settings.LastModifiedBy.Value)
                .Select(u => u.FirstName + " " + u.LastName)
                .FirstOrDefaultAsync(cancellationToken);
            updatedByUserName = user;
        }

        return settings.ThemeConfiguration.ToDto(settings.LastModifiedAt, updatedByUserName);
    }
}
