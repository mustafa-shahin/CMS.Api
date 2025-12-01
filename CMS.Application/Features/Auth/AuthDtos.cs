using CMS.Application.Features.Users.DTOs;

namespace CMS.Application.Features.Auth;

/// <summary>
/// Response DTO for authentication operations.
/// </summary>
public sealed record AuthResponse
{
    /// <summary>
    /// The JWT access token.
    /// </summary>
    public string AccessToken { get; init; } = null!;

    /// <summary>
    /// The refresh token for obtaining new access tokens.
    /// </summary>
    public string RefreshToken { get; init; } = null!;

    /// <summary>
    /// When the access token expires.
    /// </summary>
    public DateTime AccessTokenExpiresAt { get; init; }

    /// <summary>
    /// When the refresh token expires.
    /// </summary>
    public DateTime RefreshTokenExpiresAt { get; init; }

    /// <summary>
    /// Information about the authenticated user.
    /// </summary>
    public CurrentUserDto User { get; init; } = null!;
}

/// <summary>
/// Response for token refresh operations.
/// </summary>
public sealed record TokenRefreshResponse
{
    public string AccessToken { get; init; } = null!;
    public string RefreshToken { get; init; } = null!;
    public DateTime AccessTokenExpiresAt { get; init; }
    public DateTime RefreshTokenExpiresAt { get; init; }
}