namespace CMS.Application.Common.Interfaces;

/// <summary>
/// Service for accessing current authenticated user information.
/// </summary>
public interface ICurrentUserService
{
    /// <summary>
    /// Gets the current user's ID, or null if not authenticated.
    /// </summary>
    int? UserId { get; }

    /// <summary>
    /// Gets the current user's email, or null if not authenticated.
    /// </summary>
    string? Email { get; }

    /// <summary>
    /// Gets the current user's role, or null if not authenticated.
    /// </summary>
    string? Role { get; }

    /// <summary>
    /// Gets whether the current user is authenticated.
    /// </summary>
    bool IsAuthenticated { get; }

    /// <summary>
    /// Gets the IP address of the current request.
    /// </summary>
    string? IpAddress { get; }

    /// <summary>
    /// Gets the user agent of the current request.
    /// </summary>
    string? UserAgent { get; }
}