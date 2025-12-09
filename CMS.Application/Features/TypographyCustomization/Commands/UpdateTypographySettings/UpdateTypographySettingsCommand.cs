using CMS.Application.Common.Interfaces;
using CMS.Application.Features.TypographyCustomization.DTOs;
using CMS.Application.Mapping;
using CMS.Domain.Entities;
using CMS.Domain.ValueObjects;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CMS.Application.Features.TypographyCustomization.Commands.UpdateTypographySettings;

public sealed record UpdateTypographySettingsCommand : IRequest<TypographySettingsDto>
{
    public required TypographySettingsDto TypographySettings { get; init; }
}

public sealed class UpdateTypographySettingsCommandValidator : AbstractValidator<UpdateTypographySettingsCommand>
{
    public UpdateTypographySettingsCommandValidator()
    {
        RuleFor(x => x.TypographySettings).NotNull();
        RuleFor(x => x.TypographySettings.PrimaryFontFamily).NotEmpty();
        RuleFor(x => x.TypographySettings.SecondaryFontFamily).NotEmpty();
        RuleFor(x => x.TypographySettings.MonoFontFamily).NotEmpty();
        RuleFor(x => x.TypographySettings.TextStyles).NotEmpty();
    }
}

public sealed class UpdateTypographySettingsCommandHandler : IRequestHandler<UpdateTypographySettingsCommand, TypographySettingsDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<UpdateTypographySettingsCommandHandler> _logger;

    public UpdateTypographySettingsCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser,
        ILogger<UpdateTypographySettingsCommandHandler> logger)
    {
        _context = context;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<TypographySettingsDto> Handle(UpdateTypographySettingsCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating typography settings for user {UserId}", _currentUser.UserId);

        var settings = await _context.CustomizationSettings
            .Where(s => s.IsActive)
            .FirstOrDefaultAsync(cancellationToken);

        var typographySettings = request.TypographySettings.ToDomain();

        if (settings is null)
        {
            settings = CustomizationSettings.Create(
                ThemeSettings.CreateDefault(),
                typographySettings,
                LayoutSettings.CreateDefault(),
                BrandingSettings.CreateDefault()
            );
            _context.CustomizationSettings.Add(settings);
        }
        else
        {
            settings.UpdateTypography(typographySettings, _currentUser.UserId.Value);
        }

        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Typography settings saved successfully");

        string? updatedByUserName = null;
        if (settings.LastModifiedBy.HasValue)
        {
            var user = await _context.Users
                .Where(u => u.Id == settings.LastModifiedBy.Value)
                .Select(u => u.FirstName + " " + u.LastName)
                .FirstOrDefaultAsync(cancellationToken);
            updatedByUserName = user;
        }

        return settings.TypographyConfiguration.ToDto(settings.LastModifiedAt, updatedByUserName);
    }
}
