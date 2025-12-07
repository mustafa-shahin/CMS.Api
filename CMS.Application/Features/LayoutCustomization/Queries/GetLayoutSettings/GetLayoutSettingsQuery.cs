using CMS.Application.Common.Interfaces;
using CMS.Application.Features.LayoutCustomization.DTOs;
using CMS.Application.Mapping;
using CMS.Domain.ValueObjects;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CMS.Application.Features.LayoutCustomization.Queries.GetLayoutSettings;

public sealed record GetLayoutSettingsQuery : IRequest<LayoutSettingsDto>;

public sealed class GetLayoutSettingsQueryHandler : IRequestHandler<GetLayoutSettingsQuery, LayoutSettingsDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<GetLayoutSettingsQueryHandler> _logger;

    public GetLayoutSettingsQueryHandler(
        IApplicationDbContext context,
        ILogger<GetLayoutSettingsQueryHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<LayoutSettingsDto> Handle(GetLayoutSettingsQuery request, CancellationToken cancellationToken)
    {
        var settings = await _context.CustomizationSettings
            .Where(s => s.IsActive)
            .FirstOrDefaultAsync(cancellationToken);

        if (settings is null)
        {
            return LayoutSettings.CreateDefault().ToDto();
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

        return settings.LayoutConfiguration.ToDto(settings.LastModifiedAt, updatedByUserName);
    }
}
