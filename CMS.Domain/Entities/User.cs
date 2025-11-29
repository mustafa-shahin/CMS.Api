using CMS.Domain.Common;
using CMS.Domain.Enums;

namespace CMS.Domain.Entities
{
    public sealed class User : BaseAuditableEntity
    {
        public Guid Id { get; private set; }
        public string Email { get; private set; } = null!;
        public string PasswordHash { get; private set; } = null!;
        public string FirstName { get; private set; } = null!;
        public string LastName { get; private set; } = null!;
        public Enums.Enums Role { get; private set; }
        public bool IsActive { get; private set; }
        public DateTime? LastLoginAt { get; private set; }
        public int FailedLoginAttempts { get; private set; }
        public DateTime? LockoutEnd { get; private set; }

        // Navigation properties
        public ICollection<RefreshToken> RefreshTokens { get; private set; } = new List<RefreshToken>();

        // Factory method
        public static User Create(string email, string passwordHash, string firstName, string lastName, Enums.Enums role)
        {
            return new User
            {
                Id = Guid.CreateVersion7(),
                Email = email.ToLowerInvariant(),
                PasswordHash = passwordHash,
                FirstName = firstName,
                LastName = lastName,
                Role = role,
                IsActive = true,
                FailedLoginAttempts = 0
            };
        }

        // Domain methods
        public void RecordLogin() { LastLoginAt = DateTime.UtcNow; FailedLoginAttempts = 0; }
        public void RecordFailedLogin() { /* increment and check for lockout */ }
        public void Activate() { IsActive = true; }
        public void Deactivate() { IsActive = false; }
        public void UpdateProfile(string firstName, string lastName) { /* ... */ }
        public void ChangePassword(string newPasswordHash) { /* ... */ }
        public bool CanAccessDashboard() => Role is Enums.Enums.Admin or Enums.Enums.Developer;
        public bool CanAccessDesigner() => Role is Enums.Enums.Admin or Enums.Enums.Developer;
        public bool CanManageUsers() => Role is Enums.Enums.Admin;
    }
}
