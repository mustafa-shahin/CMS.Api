using CMS.Application.Common.Exceptions;
using CMS.Application.Common.Interfaces;
using CMS.Domain.Entities;
using CMS.Domain.Enums;
using CMS.Shared.Constants;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CMS.Application.Features.Users.Commands.DeleteUser;

/// <summary>
/// Command to delete a user (soft delete by deactivation).
/// </summary>
public sealed record DeleteUserCommand(int Id) : IRequest<Unit>;

/// <summary>
/// Validator for DeleteUserCommand.
/// </summary>
public sealed class DeleteUserCommandValidator : AbstractValidator<DeleteUserCommand>
{
	public DeleteUserCommandValidator()
	{
		RuleFor(x => x.Id)
			.GreaterThan(0).WithMessage("User ID must be greater than 0.");
	}
}

/// <summary>
/// Handler for DeleteUserCommand.
/// </summary>
public sealed class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand, Unit>
{
	private readonly IApplicationDbContext _context;
	private readonly ICurrentUserService _currentUserService;

	public DeleteUserCommandHandler(
		IApplicationDbContext context,
		ICurrentUserService currentUserService)
	{
		_context = context;
		_currentUserService = currentUserService;
	}

	public async Task<Unit> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
	{
		var user = await _context.Users
			.FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken);

		if (user is null)
		{
			throw new NotFoundException("User", request.Id);
		}

		// Prevent self-deletion
		if (_currentUserService.UserId == request.Id)
		{
			throw new BusinessRuleException(
				"You cannot delete your own account.",
				ErrorCodes.CannotDeleteSelf);
		}

		// Prevent deleting last admin
		if (user.Role == UserRole.Admin)
		{
			var adminCount = await _context.Users
				.CountAsync(u => u.Role == UserRole.Admin && u.IsActive, cancellationToken);

			if (adminCount <= 1)
			{
				throw new BusinessRuleException(
					"Cannot delete the last active administrator.",
					ErrorCodes.LastAdminCannotBeDeleted);
			}
		}

		// Soft delete - deactivate user
		user.Deactivate();

		// Revoke all refresh tokens
		var refreshTokens = await _context.RefreshTokens
			.Where(rt => rt.UserId == request.Id && rt.RevokedAt == null)
			.ToListAsync(cancellationToken);

		foreach (var token in refreshTokens)
		{
			token.Revoke(_currentUserService.IpAddress, "User deleted");
		}

		// Log deletion
		var auditLog = AuditLog.Create(
			_currentUserService.UserId,
			"DeleteUser",
			nameof(User),
			user.Id,
			ipAddress: _currentUserService.IpAddress,
			userAgent: _currentUserService.UserAgent,
			additionalInfo: $"Deleted user: {user.Email}");

		_context.AuditLogs.Add(auditLog);

		await _context.SaveChangesAsync(cancellationToken);

		return Unit.Value;
	}
}