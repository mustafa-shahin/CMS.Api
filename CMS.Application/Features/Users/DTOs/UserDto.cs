using CMS.Domain.Enums;

namespace CMS.Application.Features.Users.DTOs;

/// <summary>
/// DTO for detailed user information.
/// </summary>
public sealed record UserDto
{
    public int Id { get; init; }
    public string Email { get; init; } = null!;
    public string FirstName { get; init; } = null!;
    public string LastName { get; init; } = null!;
    public string FullName { get; init; } = null!;
    public UserRole Role { get; init; }
    public bool IsActive { get; init; }
    public DateTime? LastLoginAt { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? LastModifiedAt { get; init; }
}

/// <summary>
/// DTO for user list items (lightweight).
/// </summary>
public sealed record UserListDto
{
    public int Id { get; init; }
    public string Email { get; init; } = null!;
    public string FirstName { get; init; } = null!;
    public string LastName { get; init; } = null!;
    public string FullName { get; init; } = null!;
    public UserRole Role { get; init; }
    public bool IsActive { get; init; }
    public DateTime? LastLoginAt { get; init; }
    public DateTime CreatedAt { get; init; }
}

/// <summary>
/// DTO for current authenticated user.
/// </summary>
public sealed record CurrentUserDto
{
    public int Id { get; init; }
    public string Email { get; init; } = null!;
    public string FirstName { get; init; } = null!;
    public string LastName { get; init; } = null!;
    public string FullName { get; init; } = null!;
    public UserRole Role { get; init; }
    public bool CanAccessDashboard { get; init; }
    public bool CanAccessDesigner { get; init; }
    public bool CanManageUsers { get; init; }
}