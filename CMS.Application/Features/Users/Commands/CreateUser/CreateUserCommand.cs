using CMS.Application.Common.Interfaces;
using CMS.Application.Features.Users.DTOs;
using CMS.Application.Mapping;
using CMS.Domain.Entities;
using CMS.Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ValidationException = CMS.Application.Common.Exceptions.ValidationException;

namespace CMS.Application.Features.Users.Commands.CreateUser;

/// <summary>
/// Command to create a new user (Admin only).
/// </summary>
public sealed record CreateUserCommand : IRequest<UserDto>
{
    public string Email { get; init; } = null!;
    public string Password { get; init; } = null!;
    public string FirstName { get; init; } = null!;
    public string LastName { get; init; } = null!;
    public UserRole Role { get; init; }
}

/// <summary>
/// Validator for CreateUserCommand.
/// </summary>
public sealed class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.")
            .MaximumLength(256).WithMessage("Email must not exceed 256 characters.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters.")
            .MaximumLength(128).WithMessage("Password must not exceed 128 characters.")
            .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
            .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter.")
            .Matches("[0-9]").WithMessage("Password must contain at least one digit.")
            .Matches("[^a-zA-Z0-9]").WithMessage("Password must contain at least one special character.");

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required.")
            .MaximumLength(100).WithMessage("First name must not exceed 100 characters.");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required.")
            .MaximumLength(100).WithMessage("Last name must not exceed 100 characters.");

        RuleFor(x => x.Role)
            .IsInEnum().WithMessage("Invalid role specified.");
    }
}

/// <summary>
/// Handler for CreateUserCommand.
/// </summary>
public sealed class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, UserDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ICurrentUserService _currentUserService;

    public CreateUserCommandHandler(
        IApplicationDbContext context,
        IPasswordHasher passwordHasher,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _currentUserService = currentUserService;
    }

    public async Task<UserDto> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var email = request.Email.ToLowerInvariant().Trim();

        // Check if email already exists
        var emailExists = await _context.Users
            .AnyAsync(u => u.Email == email, cancellationToken);

        if (emailExists)
        {
            throw new ValidationException("Email", "An account with this email already exists.");
        }

        // Hash password
        var passwordHash = _passwordHasher.HashPassword(request.Password);

        // Create user
        var user = User.Create(
            email,
            passwordHash,
            request.FirstName.Trim(),
            request.LastName.Trim(),
            request.Role);

        _context.Users.Add(user);

        // Log creation
        var auditLog = AuditLog.Create(
            _currentUserService.UserId,
            "CreateUser",
            nameof(User),
            ipAddress: _currentUserService.IpAddress,
            userAgent: _currentUserService.UserAgent,
            additionalInfo: $"Created user with email: {email}");

        _context.AuditLogs.Add(auditLog);

        await _context.SaveChangesAsync(cancellationToken);

        return UserMapper.ToUserDto(user);
    }
}
