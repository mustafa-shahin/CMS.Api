using CMS.Application.Common.Exceptions;
using CMS.Application.Common.Interfaces;
using CMS.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CMS.Application.Features.Users.Commands.ActivateUser;

/// <summary>
/// Command to activate a user account.
/// </summary>
public sealed record ActivateUserCommand(int Id) : IRequest<Unit>;

/// <summary>
/// Validator for ActivateUserCommand.
/// </summary>
public sealed class ActivateUserCommandValidator : AbstractValidator<ActivateUserCommand>
{
    public ActivateUserCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("User ID must be greater than 0.");
    }
}

/// <summary>
/// Handler for ActivateUserCommand.
/// </summary>
public sealed class ActivateUserCommandHandler : IRequestHandler<ActivateUserCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public ActivateUserCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Unit> Handle(ActivateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken);

        if (user is null)
        {
            throw new NotFoundException("User", request.Id);
        }

        if (user.IsActive)
        {
            return Unit.Value; // Already active
        }

        user.Activate();

        // Log activation
        var auditLog = AuditLog.Create(
            _currentUserService.UserId,
            "ActivateUser",
            nameof(User),
            user.Id,
            ipAddress: _currentUserService.IpAddress,
            userAgent: _currentUserService.UserAgent,
            additionalInfo: $"Activated user: {user.Email}");

        _context.AuditLogs.Add(auditLog);

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}