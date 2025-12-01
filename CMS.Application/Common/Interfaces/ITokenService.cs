using CMS.Domain.Entities;

namespace CMS.Application.Common.Interfaces;

/// <summary>
/// Service for JWT token generation and validation.
/// </summary>
public interface ITokenService
{
    /// <summary>
    /// Generates an access token for the specified user.
    /// </summary>
    /// <param name="user">The user to generate the token for.</param>
    /// <returns>The JWT access token string.</returns>
    string GenerateAccessToken(User user);

    /// <summary>
    /// Generates a refresh token value.
    /// </summary>
    /// <returns>A tuple containing the raw token and its hash.</returns>
    (string Token, string TokenHash) GenerateRefreshToken();

    /// <summary>
    /// Validates a refresh token against its hash.
    /// </summary>
    /// <param name="token">The raw token value.</param>
    /// <param name="tokenHash">The stored hash to compare against.</param>
    /// <returns>True if the token matches the hash.</returns>
    bool ValidateRefreshToken(string token, string tokenHash);

    /// <summary>
    /// Gets the access token expiration time in minutes.
    /// </summary>
    int AccessTokenExpirationMinutes { get; }

    /// <summary>
    /// Gets the refresh token expiration time in days.
    /// </summary>
    int RefreshTokenExpirationDays { get; }
}