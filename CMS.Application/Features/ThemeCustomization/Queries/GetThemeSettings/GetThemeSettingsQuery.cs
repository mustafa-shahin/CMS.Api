using CMS.Application.Common.Interfaces;
using CMS.Application.Features.ThemeCustomization.DTOs;
using CMS.Application.Mapping;
using CMS.Domain.ValueObjects;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CMS.Application.Features.ThemeCustomization.Queries.GetThemeSettings;

/// <summary>
/// Query to get current theme settings.
/// </summary>
public sealed record GetThemeSettingsQuery : IRequest<ThemeSettingsDto>;

/// <summary>
/// Handler for GetThemeSettingsQuery
/// </summary>
public sealed class GetThemeSettingsQueryHandler : IRequestHandler<GetThemeSettingsQuery, ThemeSettingsDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<GetThemeSettingsQueryHandler> _logger;

    public GetThemeSettingsQueryHandler(
        IApplicationDbContext context,
        ILogger<GetThemeSettingsQueryHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ThemeSettingsDto> Handle(GetThemeSettingsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving theme settings");

        var settings = await _context.CustomizationSettings
            .Where(s => s.IsActive)
            .FirstOrDefaultAsync(cancellationToken);

        if (settings is null)
        {
            _logger.LogInformation("No active customization settings found, returning defaults");
            return ThemeSettings.CreateDefault().ToDto();
        }

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
