using CMS.Application.Common.Exceptions;
using CMS.Application.Common.Interfaces;
using CMS.Application.Mapping;
using CMS.Domain.Entities;
using CMS.Shared.Constants;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CMS.Application.Features.Auth.Commands.Login;

/// <summary>
/// Command for user login.
/// </summary>
public sealed record LoginCommand : IRequest<AuthResponse>
{
    public string Email { get; init; } = null!;
    public string Password { get; init; } = null!;
}

/// <summary>
/// Validator for LoginCommand.
/// </summary>
public sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.")
            .MaximumLength(256).WithMessage("Email must not exceed 256 characters.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.");
    }
}

/// <summary>
/// Handler for LoginCommand.
/// </summary>
public sealed class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;
    private readonly ICurrentUserService _currentUserService;

    public LoginCommandHandler(
        IApplicationDbContext context,
        IPasswordHasher passwordHasher,
        ITokenService tokenService,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
        _currentUserService = currentUserService;
    }

    public async Task<AuthResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var email = request.Email.ToLowerInvariant().Trim();

        var user = await _context.Users
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

        if (user is null)
        {
            throw new UnauthorizedException("Invalid email or password.", ErrorCodes.InvalidCredentials);
        }

        if (!user.IsActive)
        {
            throw new UnauthorizedException("Your account has been deactivated.", ErrorCodes.AccountInactive);
        }

        if (user.IsLockedOut)
        {
            throw new UnauthorizedException(
                $"Account is locked. Please try again after {user.LockoutEnd:HH:mm:ss}.",
                ErrorCodes.AccountLocked);
        }

        if (!_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
        {
            user.RecordFailedLogin();
            await _context.SaveChangesAsync(cancellationToken);

            throw new UnauthorizedException("Invalid email or password.", ErrorCodes.InvalidCredentials);
        }

        // Record successful login
        user.RecordSuccessfulLogin();

        // Generate tokens
        var accessToken = _tokenService.GenerateAccessToken(user);
        var (refreshToken, refreshTokenHash) = _tokenService.GenerateRefreshToken();

        // Create and store refresh token
        var refreshTokenEntity = RefreshToken.Create(
            user.Id,
            refreshTokenHash,
            _tokenService.RefreshTokenExpirationDays,
            _currentUserService.IpAddress);

        user.RefreshTokens.Add(refreshTokenEntity);

        // Log the login
        var auditLog = AuditLog.Create(
            user.Id,
            "Login",
            nameof(User),
            user.Id,
            ipAddress: _currentUserService.IpAddress,
            userAgent: _currentUserService.UserAgent);

        _context.AuditLogs.Add(auditLog);

        await _context.SaveChangesAsync(cancellationToken);

        return new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            AccessTokenExpiresAt = DateTime.UtcNow.AddMinutes(_tokenService.AccessTokenExpirationMinutes),
            RefreshTokenExpiresAt = refreshTokenEntity.ExpiresAt,
            User = UserMapper.ToCurrentUserDto(user)
        };
    }
}