using CMS.Application.Common.Interfaces;
using CMS.Application.Features.LayoutCustomization.DTOs;
using CMS.Application.Mapping;
using CMS.Domain.Entities;
using CMS.Domain.ValueObjects;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CMS.Application.Features.LayoutCustomization.Commands.UpdateLayoutSettings;

public sealed record UpdateLayoutSettingsCommand : IRequest<LayoutSettingsDto>
{
    public required LayoutSettingsDto LayoutSettings { get; init; }
}

public sealed class UpdateLayoutSettingsCommandValidator : AbstractValidator<UpdateLayoutSettingsCommand>
{
    public UpdateLayoutSettingsCommandValidator()
    {
        RuleFor(x => x.LayoutSettings).NotNull();
        RuleFor(x => x.LayoutSettings.HeaderConfiguration).NotNull();
        RuleFor(x => x.LayoutSettings.FooterConfiguration).NotNull();
        RuleFor(x => x.LayoutSettings.Spacing).NotNull();
        RuleFor(x => x.LayoutSettings.Spacing.ContainerMaxWidth)
            .InclusiveBetween(640, 1920);
        RuleFor(x => x.LayoutSettings.FooterConfiguration.ColumnCount)
            .InclusiveBetween(1, 4);
    }
}

public sealed class UpdateLayoutSettingsCommandHandler : IRequestHandler<UpdateLayoutSettingsCommand, LayoutSettingsDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<UpdateLayoutSettingsCommandHandler> _logger;

    public UpdateLayoutSettingsCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser,
        ILogger<UpdateLayoutSettingsCommandHandler> logger)
    {
        _context = context;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<LayoutSettingsDto> Handle(UpdateLayoutSettingsCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating layout settings for user {UserId}", _currentUser.UserId);

        var settings = await _context.CustomizationSettings
            .Where(s => s.IsActive)
            .FirstOrDefaultAsync(cancellationToken);

        var layoutSettings = request.LayoutSettings.ToDomain();

        if (settings is null)
        {
            settings = CustomizationSettings.Create(
                ThemeSettings.CreateDefault(),
                TypographySettings.CreateDefault(),
                layoutSettings
            );
            _context.CustomizationSettings.Add(settings);
        }
        else
        {
            settings.UpdateLayout(layoutSettings, _currentUser.UserId.Value);
        }

        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Layout settings saved successfully");

        string? updatedByUserName = null;
        if (settings.LastModifiedBy.HasValue)
        {
            var user = await _context.Users
                .Where(u => u.Id == settings.LastModifiedBy.Value)
                .Select(u => u.FirstName + " " + u.LastName)
                .FirstOrDefaultAsync(cancellationToken);
            updatedByUserName = user;
        }

        return settings.LayoutConfiguration.ToDto(settings.LastModifiedAt, updatedByUserName);
    }
}
