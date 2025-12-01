using CMS.Application.Common.Interfaces;
using CMS.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CMS.Application.Features.Auth.Commands.Logout;

/// <summary>
/// Command for user logout (revokes refresh token).
/// </summary>
public sealed record LogoutCommand : IRequest<Unit>
{
    public string? RefreshToken { get; init; }
    public bool RevokeAllTokens { get; init; }
}

/// <summary>
/// Validator for LogoutCommand.
/// </summary>
public sealed class LogoutCommandValidator : AbstractValidator<LogoutCommand>
{
    public LogoutCommandValidator()
    {
        // RefreshToken is optional - if not provided and user is authenticated,
        // we can revoke all their tokens based on RevokeAllTokens flag
    }
}

/// <summary>
/// Handler for LogoutCommand.
/// </summary>
public sealed class LogoutCommandHandler : IRequestHandler<LogoutCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly ITokenService _tokenService;
    private readonly ICurrentUserService _currentUserService;

    public LogoutCommandHandler(
        IApplicationDbContext context,
        ITokenService tokenService,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _tokenService = tokenService;
        _currentUserService = currentUserService;
    }

    public async Task<Unit> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        if (request.RevokeAllTokens && _currentUserService.UserId.HasValue)
        {
            // Revoke all tokens for the current user
            await RevokeAllUserTokensAsync(_currentUserService.UserId.Value, cancellationToken);
        }
        else if (!string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            // Revoke specific token
            await RevokeTokenAsync(request.RefreshToken, cancellationToken);
        }

        // Log logout
        if (_currentUserService.UserId.HasValue)
        {
            var auditLog = AuditLog.Create(
                _currentUserService.UserId.Value,
                "Logout",
                nameof(User),
                _currentUserService.UserId.Value,
                ipAddress: _currentUserService.IpAddress,
                userAgent: _currentUserService.UserAgent);

            _context.AuditLogs.Add(auditLog);
            await _context.SaveChangesAsync(cancellationToken);
        }

        return Unit.Value;
    }

    private async Task RevokeTokenAsync(string refreshToken, CancellationToken cancellationToken)
    {
        var tokens = await _context.RefreshTokens
            .Where(rt => rt.RevokedAt == null)
            .ToListAsync(cancellationToken);

        var existingToken = tokens
            .FirstOrDefault(rt => _tokenService.ValidateRefreshToken(refreshToken, rt.TokenHash));

        if (existingToken is not null)
        {
            existingToken.Revoke(_currentUserService.IpAddress, "User logged out");
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    private async Task RevokeAllUserTokensAsync(int userId, CancellationToken cancellationToken)
    {
        var activeTokens = await _context.RefreshTokens
            .Where(rt => rt.UserId == userId && rt.RevokedAt == null)
            .ToListAsync(cancellationToken);

        foreach (var token in activeTokens)
        {
            token.Revoke(_currentUserService.IpAddress, "User logged out from all devices");
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}