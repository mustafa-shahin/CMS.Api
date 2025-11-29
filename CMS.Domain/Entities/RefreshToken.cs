using System;
using System.Collections.Generic;
using System.Text;

namespace CMS.Domain.Entities
{
    public sealed class RefreshToken
    {
        public Guid Id { get; private set; }
        public Guid UserId { get; private set; }
        public string TokenHash { get; private set; } = null!;
        public DateTime ExpiresAt { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime? RevokedAt { get; private set; }
        public string? ReplacedByTokenHash { get; private set; }
        public string? CreatedByIp { get; private set; }
        public string? RevokedByIp { get; private set; }

        // Navigation
        public User User { get; private set; } = null!;

        public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
        public bool IsRevoked => RevokedAt is not null;
        public bool IsActive => !IsRevoked && !IsExpired;

        public static RefreshToken Create(Guid userId, string tokenHash, int expirationDays, string? ipAddress)
        {
            return new RefreshToken
            {
                Id = Guid.CreateVersion7(),
                UserId = userId,
                TokenHash = tokenHash,
                ExpiresAt = DateTime.UtcNow.AddDays(expirationDays),
                CreatedAt = DateTime.UtcNow,
                CreatedByIp = ipAddress
            };
        }

        public void Revoke(string? ipAddress, string? replacedByTokenHash = null)
        {
            RevokedAt = DateTime.UtcNow;
            RevokedByIp = ipAddress;
            ReplacedByTokenHash = replacedByTokenHash;
        }
    }
}
