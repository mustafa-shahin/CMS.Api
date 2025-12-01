namespace CMS.Application.Common.Interfaces;

/// <summary>
/// Service for secure password hashing using Argon2id.
/// </summary>
public interface IPasswordHasher
{
    /// <summary>
    /// Hashes a password using Argon2id.
    /// </summary>
    /// <param name="password">The plaintext password to hash.</param>
    /// <returns>The Argon2id hash string.</returns>
    string HashPassword(string password);

    /// <summary>
    /// Verifies a password against a stored hash.
    /// </summary>
    /// <param name="password">The plaintext password to verify.</param>
    /// <param name="passwordHash">The stored hash to verify against.</param>
    /// <returns>True if the password matches the hash.</returns>
    bool VerifyPassword(string password, string passwordHash);
}