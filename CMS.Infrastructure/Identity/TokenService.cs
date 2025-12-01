using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using CMS.Application.Common.Interfaces;
using CMS.Domain.Constants;
using CMS.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace CMS.Infrastructure.Identity;

/// <summary>
/// Service for JWT token generation and validation.
/// </summary>
public sealed class TokenService : ITokenService
{
    private readonly string _secretKey;
    private readonly string _issuer;
    private readonly string _audience;

    public int AccessTokenExpirationMinutes { get; }
    public int RefreshTokenExpirationDays { get; }

    public TokenService(IConfiguration configuration)
    {
        _secretKey = configuration["Jwt:Key"]
            ?? throw new InvalidOperationException("JWT Key is not configured.");
        _issuer = configuration["Jwt:Issuer"]
            ?? throw new InvalidOperationException("JWT Issuer is not configured.");
        _audience = configuration["Jwt:Audience"]
            ?? throw new InvalidOperationException("JWT Audience is not configured.");

        AccessTokenExpirationMinutes = configuration.GetValue<int>("Jwt:AccessTokenExpirationMinutes", 15);
        RefreshTokenExpirationDays = configuration.GetValue<int>("Jwt:RefreshTokenExpirationDays", 7);
    }

    /// <summary>
    /// Generates a JWT access token for the specified user.
    /// </summary>
    public string GenerateAccessToken(User user)
    {
        var claims = new Dictionary<string, object>
        {
            [JwtRegisteredClaimNames.Sub] = user.Id.ToString(),
            [JwtRegisteredClaimNames.Email] = user.Email,
            [JwtRegisteredClaimNames.Jti] = Guid.NewGuid().ToString(),
            [JwtRegisteredClaimNames.Iat] = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            [Permissions.UserIdClaimType] = user.Id.ToString(),
            [Permissions.EmailClaimType] = user.Email,
            [Permissions.RoleClaimType] = user.Role.ToString(),
            [Permissions.FullNameClaimType] = user.FullName,
            [ClaimTypes.Role] = user.Role.ToString()
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Issuer = _issuer,
            Audience = _audience,
            Claims = claims,
            Expires = DateTime.UtcNow.AddMinutes(AccessTokenExpirationMinutes),
            SigningCredentials = credentials
        };

        var handler = new JsonWebTokenHandler();
        return handler.CreateToken(tokenDescriptor);
    }

    /// <summary>
    /// Generates a cryptographically secure refresh token.
    /// </summary>
    public (string Token, string TokenHash) GenerateRefreshToken()
    {
        var randomBytes = RandomNumberGenerator.GetBytes(64);
        var token = Convert.ToBase64String(randomBytes);
        var tokenHash = HashToken(token);

        return (token, tokenHash);
    }

    /// <summary>
    /// Validates a refresh token against its stored hash.
    /// </summary>
    public bool ValidateRefreshToken(string token, string tokenHash)
    {
        if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(tokenHash))
        {
            return false;
        }

        var computedHash = HashToken(token);
        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(computedHash),
            Encoding.UTF8.GetBytes(tokenHash));
    }

    private static string HashToken(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToBase64String(bytes);
    }
}