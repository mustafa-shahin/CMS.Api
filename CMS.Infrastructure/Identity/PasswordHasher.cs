using CMS.Application.Common.Interfaces;
using Isopoh.Cryptography.Argon2;
using Konscious.Security.Cryptography;
using System.Security.Cryptography;
using System.Text;

namespace CMS.Infrastructure.Identity;

/// <summary>
/// Password hashing service using Argon2id algorithm.
/// OWASP recommended parameters for Argon2id.
/// </summary>
public sealed class PasswordHasher : IPasswordHasher
{
    private const int SaltSize = 16;       // 128 bits
    private const int HashSize = 32;       // 256 bits
    private const int DegreeOfParallelism = 4;
    private const int MemorySize = 65536;  // 64 MB
    private const int Iterations = 3;

    /// <summary>
    /// Hashes a password using Argon2id with a random salt.
    /// </summary>
    public string HashPassword(string password)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(password);

        // Generate a random salt
        var salt = RandomNumberGenerator.GetBytes(SaltSize);

        // Hash the password
        var hash = HashPasswordWithSalt(password, salt);

        // Combine salt and hash for storage
        // Format: $argon2id$v=19$m=65536,t=3,p=4$<base64-salt>$<base64-hash>
        return $"$argon2id$v=19$m={MemorySize},t={Iterations},p={DegreeOfParallelism}${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
    }

    /// <summary>
    /// Verifies a password against a stored Argon2id hash.
    /// </summary>
    public bool VerifyPassword(string password, string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(passwordHash))
        {
            return false;
        }

        try
        {
            // Parse the hash string
            var parts = passwordHash.Split('$');
            if (parts.Length != 6 || parts[1] != "argon2id")
            {
                return false;
            }

            // Extract salt and hash
            var salt = Convert.FromBase64String(parts[4]);
            var storedHash = Convert.FromBase64String(parts[5]);

            // Compute hash with the same salt
            var computedHash = HashPasswordWithSalt(password, salt);

            // Constant-time comparison to prevent timing attacks
            return CryptographicOperations.FixedTimeEquals(storedHash, computedHash);
        }
        catch
        {
            // Invalid hash format
            return false;
        }
    }

    private static byte[] HashPasswordWithSalt(string password, byte[] salt)
    {
        using var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password))
        {
            Salt = salt,
            DegreeOfParallelism = DegreeOfParallelism,
            MemorySize = MemorySize,
            Iterations = Iterations
        };

        return argon2.GetBytes(HashSize);
    }
}