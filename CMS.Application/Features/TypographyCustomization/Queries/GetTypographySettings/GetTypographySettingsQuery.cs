using CMS.Application.Common.Interfaces;
using CMS.Application.Features.TypographyCustomization.DTOs;
using CMS.Application.Mapping;
using CMS.Domain.ValueObjects;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CMS.Application.Features.TypographyCustomization.Queries.GetTypographySettings;

public sealed record GetTypographySettingsQuery : IRequest<TypographySettingsDto>;

public sealed class GetTypographySettingsQueryHandler : IRequestHandler<GetTypographySettingsQuery, TypographySettingsDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<GetTypographySettingsQueryHandler> _logger;

    public GetTypographySettingsQueryHandler(
        IApplicationDbContext context,
        ILogger<GetTypographySettingsQueryHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<TypographySettingsDto> Handle(GetTypographySettingsQuery request, CancellationToken cancellationToken)
    {
        var settings = await _context.CustomizationSettings
            .Where(s => s.IsActive)
            .FirstOrDefaultAsync(cancellationToken);

        if (settings is null)
        {
            return TypographySettings.CreateDefault().ToDto();
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

        return settings.TypographyConfiguration.ToDto(settings.LastModifiedAt, updatedByUserName);
    }
}
