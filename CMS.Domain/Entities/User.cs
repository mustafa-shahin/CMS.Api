using CMS.Domain.Common;
using CMS.Domain.Enums;

namespace CMS.Domain.Entities;

/// <summary>
/// Represents a user in the CMS system.
/// Contains authentication, authorization, and profile information.
/// </summary>
public sealed class User : BaseAuditableEntity
{
    private const int MaxFailedLoginAttempts = 5;
    private static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(15);

    /// <summary>
    /// User's email address (used as unique identifier for login).
    /// Always stored in lowercase for case-insensitive matching.
    /// </summary>
    public string Email { get; private set; } = null!;

    /// <summary>
    /// Argon2id hashed password.
    /// </summary>
    public string PasswordHash { get; private set; } = null!;

    /// <summary>
    /// User's first name.
    /// </summary>
    public string FirstName { get; private set; } = null!;

    /// <summary>
    /// User's last name.
    /// </summary>
    public string LastName { get; private set; } = null!;

    /// <summary>
    /// User's role determining access permissions.
    /// </summary>
    public UserRole Role { get; private set; }

    /// <summary>
    /// Indicates whether the user account is active.
    /// Inactive users cannot log in.
    /// </summary>
    public bool IsActive { get; private set; }

    /// <summary>
    /// Timestamp of the user's last successful login.
    /// </summary>
    public DateTime? LastLoginAt { get; private set; }

    /// <summary>
    /// Number of consecutive failed login attempts.
    /// Used for account lockout protection.
    /// </summary>
    public int FailedLoginAttempts { get; private set; }

    /// <summary>
    /// UTC timestamp when the account lockout expires.
    /// Null if account is not locked.
    /// </summary>
    public DateTime? LockoutEnd { get; private set; }

    /// <summary>
    /// Navigation property for refresh tokens.
    /// </summary>
    public ICollection<RefreshToken> RefreshTokens { get; private set; } = new List<RefreshToken>();

    /// <summary>
    /// Private constructor for EF Core.
    /// </summary>
    private User() { }

    /// <summary>
    /// Factory method to create a new user with validated data.
    /// </summary>
    public static User Create(
        string email,
        string passwordHash,
        string firstName,
        string lastName,
        UserRole role)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        ArgumentException.ThrowIfNullOrWhiteSpace(passwordHash);
        ArgumentException.ThrowIfNullOrWhiteSpace(firstName);
        ArgumentException.ThrowIfNullOrWhiteSpace(lastName);

        return new User
        {
            Email = email.ToLowerInvariant().Trim(),
            PasswordHash = passwordHash,
            FirstName = firstName.Trim(),
            LastName = lastName.Trim(),
            Role = role,
            IsActive = true,
            FailedLoginAttempts = 0
        };
    }

    /// <summary>
    /// Gets the user's full name.
    /// </summary>
    public string FullName => $"{FirstName} {LastName}";

    /// <summary>
    /// Checks if the account is currently locked out.
    /// </summary>
    public bool IsLockedOut => LockoutEnd.HasValue && LockoutEnd.Value > DateTime.UtcNow;

    /// <summary>
    /// Records a successful login attempt.
    /// Resets failed login counter and updates last login timestamp.
    /// </summary>
    public void RecordSuccessfulLogin()
    {
        LastLoginAt = DateTime.UtcNow;
        FailedLoginAttempts = 0;
        LockoutEnd = null;
    }

    /// <summary>
    /// Records a failed login attempt.
    /// May trigger account lockout if threshold is exceeded.
    /// </summary>
    public void RecordFailedLogin()
    {
        FailedLoginAttempts++;

        if (FailedLoginAttempts >= MaxFailedLoginAttempts)
        {
            LockoutEnd = DateTime.UtcNow.Add(LockoutDuration);
        }
    }

    /// <summary>
    /// Activates the user account.
    /// </summary>
    public void Activate()
    {
        IsActive = true;
        LockoutEnd = null;
        FailedLoginAttempts = 0;
    }

    /// <summary>
    /// Deactivates the user account.
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
    }

    /// <summary>
    /// Updates the user's profile information.
    /// </summary>
    public void UpdateProfile(string firstName, string lastName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(firstName);
        ArgumentException.ThrowIfNullOrWhiteSpace(lastName);

        FirstName = firstName.Trim();
        LastName = lastName.Trim();
    }

    /// <summary>
    /// Changes the user's password.
    /// </summary>
    public void ChangePassword(string newPasswordHash)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(newPasswordHash);
        PasswordHash = newPasswordHash;
    }

    /// <summary>
    /// Updates the user's role.
    /// </summary>
    public void UpdateRole(UserRole newRole)
    {
        Role = newRole;
    }

    /// <summary>
    /// Checks if the user can access the admin dashboard.
    /// </summary>
    public bool CanAccessDashboard() => Role is UserRole.Admin or UserRole.Developer;

    /// <summary>
    /// Checks if the user can access the page designer.
    /// </summary>
    public bool CanAccessDesigner() => Role is UserRole.Admin or UserRole.Developer;

    /// <summary>
    /// Checks if the user can manage other users.
    /// </summary>
    public bool CanManageUsers() => Role is UserRole.Admin;
}