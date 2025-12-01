using CMS.Application.Common.Exceptions;
using CMS.Application.Common.Interfaces;
using CMS.Domain.Entities;
using CMS.Domain.Enums;
using CMS.Shared.Constants;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CMS.Application.Features.Users.Commands.DeactivateUser;

/// <summary>
/// Command to deactivate a user account.
/// </summary>
public sealed record DeactivateUserCommand(int Id) : IRequest<Unit>;

/// <summary>
/// Validator for DeactivateUserCommand.
/// </summary>
public sealed class DeactivateUserCommandValidator : AbstractValidator<DeactivateUserCommand>
{
    public DeactivateUserCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("User ID must be greater than 0.");
    }
}

/// <summary>
/// Handler for DeactivateUserCommand.
/// </summary>
public sealed class DeactivateUserCommandHandler : IRequestHandler<DeactivateUserCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public DeactivateUserCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Unit> Handle(DeactivateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken);

        if (user is null)
        {
            throw new NotFoundException("User", request.Id);
        }

        // Prevent self-deactivation
        if (_currentUserService.UserId == request.Id)
        {
            throw new BusinessRuleException(
                "You cannot deactivate your own account.",
                ErrorCodes.CannotDeactivateSelf);
        }

        // Prevent deactivating last admin
        if (user.Role == UserRole.Admin)
        {
            var adminCount = await _context.Users
                .CountAsync(u => u.Role == UserRole.Admin && u.IsActive, cancellationToken);

            if (adminCount <= 1)
            {
                throw new BusinessRuleException(
                    "Cannot deactivate the last active administrator.",
                    ErrorCodes.LastAdminCannotBeDeleted);
            }
        }

        if (!user.IsActive)
        {
            return Unit.Value; // Already inactive
        }

        user.Deactivate();

        // Revoke all refresh tokens
        var refreshTokens = await _context.RefreshTokens
            .Where(rt => rt.UserId == request.Id && rt.RevokedAt == null)
            .ToListAsync(cancellationToken);

        foreach (var token in refreshTokens)
        {
            token.Revoke(_currentUserService.IpAddress, "User deactivated");
        }

        // Log deactivation
        var auditLog = AuditLog.Create(
            _currentUserService.UserId,
            "DeactivateUser",
            nameof(User),
            user.Id,
            ipAddress: _currentUserService.IpAddress,
            userAgent: _currentUserService.UserAgent,
            additionalInfo: $"Deactivated user: {user.Email}");

        _context.AuditLogs.Add(auditLog);

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}