using CMS.Application.Common.Interfaces;
using Isopoh.Cryptography.Argon2;

namespace CMS.Infrastructure.Identity;

/// <summary>
/// Password hashing service using Argon2id algorithm.
/// </summary>
public sealed class PasswordHasher : IPasswordHasher
{
    private readonly Argon2Config _config;

    public PasswordHasher()
    {
        // OWASP recommended settings for Argon2id
        _config = new Argon2Config
        {
            Type = Argon2Type.HybridAddressing, // Argon2id
            Version = Argon2Version.Nineteen,
            TimeCost = 3,           // Number of iterations
            MemoryCost = 65536,     // 64 MB memory
            Lanes = 4,              // Parallelism
            Threads = 4,
            HashLength = 32         // 256 bits
        };
    }

    /// <summary>
    /// Hashes a password using Argon2id.
    /// </summary>
    public string HashPassword(string password)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(password);

        return Argon2.Hash(_config, password);
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
            return Argon2.Verify(passwordHash, password);
        }
        catch
        {
            // Invalid hash format
            return false;
        }
    }
}