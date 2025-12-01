namespace CMS.Domain.Entities;

/// <summary>
/// Represents a refresh token for JWT token renewal.
/// Implements token rotation and revocation tracking for security.
/// </summary>
public sealed class RefreshToken
{
    /// <summary>
    /// Unique identifier for the refresh token.
    /// </summary>
    public int Id { get; private set; }

    /// <summary>
    /// The user this token belongs to.
    /// </summary>
    public int UserId { get; private set; }

    /// <summary>
    /// SHA-256 hash of the actual token value.
    /// The raw token is only sent to the client and never stored.
    /// </summary>
    public string TokenHash { get; private set; } = null!;

    /// <summary>
    /// UTC timestamp when this token expires.
    /// </summary>
    public DateTime ExpiresAt { get; private set; }

    /// <summary>
    /// UTC timestamp when this token was created.
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// UTC timestamp when this token was revoked.
    /// Null if the token has not been revoked.
    /// </summary>
    public DateTime? RevokedAt { get; private set; }

    /// <summary>
    /// Hash of the token that replaced this one during rotation.
    /// Used for detecting token reuse attacks.
    /// </summary>
    public string? ReplacedByTokenHash { get; private set; }

    /// <summary>
    /// IP address from which this token was created.
    /// </summary>
    public string? CreatedByIp { get; private set; }

    /// <summary>
    /// IP address from which this token was revoked.
    /// </summary>
    public string? RevokedByIp { get; private set; }

    /// <summary>
    /// Reason for revocation.
    /// </summary>
    public string? RevocationReason { get; private set; }

    /// <summary>
    /// Navigation property to the user.
    /// </summary>
    public User User { get; private set; } = null!;

    /// <summary>
    /// Checks if the token has expired.
    /// </summary>
    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;

    /// <summary>
    /// Checks if the token has been revoked.
    /// </summary>
    public bool IsRevoked => RevokedAt.HasValue;

    /// <summary>
    /// Checks if the token is currently active (not revoked and not expired).
    /// </summary>
    public bool IsActive => !IsRevoked && !IsExpired;

    /// <summary>
    /// Private constructor for EF Core.
    /// </summary>
    private RefreshToken() { }

    /// <summary>
    /// Creates a new refresh token.
    /// </summary>
    public static RefreshToken Create(
        int userId,
        string tokenHash,
        int expirationDays,
        string? ipAddress)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tokenHash);

        if (expirationDays <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(expirationDays), "Expiration days must be positive.");
        }

        return new RefreshToken
        {
            UserId = userId,
            TokenHash = tokenHash,
            ExpiresAt = DateTime.UtcNow.AddDays(expirationDays),
            CreatedAt = DateTime.UtcNow,
            CreatedByIp = ipAddress
        };
    }

    /// <summary>
    /// Revokes this token.
    /// </summary>
    public void Revoke(string? ipAddress, string? reason = null, string? replacedByTokenHash = null)
    {
        RevokedAt = DateTime.UtcNow;
        RevokedByIp = ipAddress;
        RevocationReason = reason;
        ReplacedByTokenHash = replacedByTokenHash;
    }
}