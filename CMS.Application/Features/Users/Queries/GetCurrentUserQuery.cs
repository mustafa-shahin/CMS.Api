using CMS.Application.Common.Exceptions;
using CMS.Application.Common.Interfaces;
using CMS.Application.Features.Users.DTOs;
using CMS.Application.Mapping;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CMS.Application.Features.Users.Queries;

/// <summary>
/// Query to get the currently authenticated user's information.
/// </summary>
public sealed record GetCurrentUserQuery : IRequest<CurrentUserDto>;

/// <summary>
/// Handler for GetCurrentUserQuery.
/// </summary>
public sealed class GetCurrentUserQueryHandler : IRequestHandler<GetCurrentUserQuery, CurrentUserDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetCurrentUserQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<CurrentUserDto> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.UserId.HasValue)
        {
            throw new UnauthorizedException("User is not authenticated.");
        }

        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == _currentUserService.UserId.Value, cancellationToken);

        if (user is null)
        {
            throw new NotFoundException("User", _currentUserService.UserId.Value);
        }

        return UserMapper.ToCurrentUserDto(user);
    }
}