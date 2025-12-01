using CMS.Application.Common.Exceptions;
using CMS.Application.Common.Interfaces;
using CMS.Application.Features.Users.DTOs;
using CMS.Application.Mapping;
using CMS.Domain.Entities;
using CMS.Domain.Enums;
using CMS.Shared.Constants;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CMS.Application.Features.Users.Commands.UpdateUser;

/// <summary>
/// Command to update an existing user.
/// </summary>
public sealed record UpdateUserCommand : IRequest<UserDto>
{
    public int Id { get; init; }
    public string FirstName { get; init; } = null!;
    public string LastName { get; init; } = null!;
    public UserRole? Role { get; init; }
}

/// <summary>
/// Validator for UpdateUserCommand.
/// </summary>
public sealed class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("User ID must be greater than 0.");

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required.")
            .MaximumLength(100).WithMessage("First name must not exceed 100 characters.");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required.")
            .MaximumLength(100).WithMessage("Last name must not exceed 100 characters.");

        RuleFor(x => x.Role)
            .IsInEnum().WithMessage("Invalid role specified.")
            .When(x => x.Role.HasValue);
    }
}

/// <summary>
/// Handler for UpdateUserCommand.
/// </summary>
public sealed class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, UserDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public UpdateUserCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<UserDto> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken);

        if (user is null)
        {
            throw new NotFoundException("User", request.Id);
        }

        // Prevent changing own role
        if (request.Role.HasValue &&
            _currentUserService.UserId == request.Id &&
            user.Role != request.Role.Value)
        {
            throw new BusinessRuleException(
                "You cannot change your own role.",
                ErrorCodes.CannotChangeOwnRole);
        }

        // Update profile
        user.UpdateProfile(request.FirstName.Trim(), request.LastName.Trim());

        // Update role if specified
        if (request.Role.HasValue)
        {
            user.UpdateRole(request.Role.Value);
        }

        // Log update
        var auditLog = AuditLog.Create(
            _currentUserService.UserId,
            "UpdateUser",
            nameof(User),
            user.Id,
            ipAddress: _currentUserService.IpAddress,
            userAgent: _currentUserService.UserAgent);

        _context.AuditLogs.Add(auditLog);

        await _context.SaveChangesAsync(cancellationToken);

        return UserMapper.ToUserDto(user);
    }
}