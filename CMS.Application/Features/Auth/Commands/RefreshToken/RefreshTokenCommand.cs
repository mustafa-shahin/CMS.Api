using CMS.Application.Common.Exceptions;
using CMS.Application.Common.Interfaces;
using CMS.Application.Features.Auth;
using CMS.Domain.Entities;
using CMS.Shared.Constants;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CMS.Application.Features.Auth.Commands.RefreshTokens;

/// <summary>
/// Command for refreshing JWT tokens.
/// </summary>
public sealed record RefreshTokenCommand : IRequest<TokenRefreshResponse>
{
    public string RefreshToken { get; init; } = null!;
}

/// <summary>
/// Validator for RefreshTokenCommand.
/// </summary>
public sealed class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage("Refresh token is required.");
    }
}

/// <summary>
/// Handler for RefreshTokenCommand with token rotation.
/// </summary>
public sealed class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, TokenRefreshResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly ITokenService _tokenService;
    private readonly ICurrentUserService _currentUserService;

    public RefreshTokenCommandHandler(
        IApplicationDbContext context,
        ITokenService tokenService,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _tokenService = tokenService;
        _currentUserService = currentUserService;
    }

    public async Task<TokenRefreshResponse> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        // Find refresh token by comparing hash
        var refreshTokens = await _context.RefreshTokens
            .Include(rt => rt.User)
            .Where(rt => rt.RevokedAt == null)
            .ToListAsync(cancellationToken);

        var existingToken = refreshTokens
            .FirstOrDefault(rt => _tokenService.ValidateRefreshToken(request.RefreshToken, rt.TokenHash));

        if (existingToken is null)
        {
            throw new UnauthorizedException("Invalid refresh token.", ErrorCodes.RefreshTokenInvalid);
        }

        if (existingToken.IsExpired)
        {
            throw new UnauthorizedException("Refresh token has expired.", ErrorCodes.RefreshTokenExpired);
        }

        if (existingToken.IsRevoked)
        {
            // Token reuse detected - revoke all tokens for this user
            await RevokeAllUserTokensAsync(existingToken.UserId, "Token reuse detected", cancellationToken);
            throw new UnauthorizedException("Refresh token has been revoked.", ErrorCodes.RefreshTokenRevoked);
        }

        var user = existingToken.User;

        if (!user.IsActive)
        {
            throw new UnauthorizedException("Account has been deactivated.", ErrorCodes.AccountInactive);
        }

        // Generate new tokens (token rotation)
        var newAccessToken = _tokenService.GenerateAccessToken(user);
        var (newRefreshToken, newRefreshTokenHash) = _tokenService.GenerateRefreshToken();

        // Revoke old token
        existingToken.Revoke(
            _currentUserService.IpAddress,
            "Replaced by new token",
            newRefreshTokenHash);

        // Create new refresh token
        var newRefreshTokenEntity = RefreshToken.Create(
            user.Id,
            newRefreshTokenHash,
            _tokenService.RefreshTokenExpirationDays,
            _currentUserService.IpAddress);

        _context.RefreshTokens.Add(newRefreshTokenEntity);
        await _context.SaveChangesAsync(cancellationToken);

        return new TokenRefreshResponse
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken,
            AccessTokenExpiresAt = DateTime.UtcNow.AddMinutes(_tokenService.AccessTokenExpirationMinutes),
            RefreshTokenExpiresAt = newRefreshTokenEntity.ExpiresAt
        };
    }

    private async Task RevokeAllUserTokensAsync(int userId, string reason, CancellationToken cancellationToken)
    {
        var activeTokens = await _context.RefreshTokens
            .Where(rt => rt.UserId == userId && rt.RevokedAt == null)
            .ToListAsync(cancellationToken);

        foreach (var token in activeTokens)
        {
            token.Revoke(_currentUserService.IpAddress, reason);
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}