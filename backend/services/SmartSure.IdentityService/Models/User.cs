using System.ComponentModel.DataAnnotations;

namespace IdentityService.Models
{
    /// <summary>
    /// Represent or implements User.
    /// </summary>
    public class User
    {
        [Key]
        public Guid UserId { get; set; }

        [Required]
        public string FullName { get; set; }

        [EmailAddress]
        [Required]
        public string Email { get; set; }

        [Required]
        public string PhoneNumber { get; set; }

        public string Address { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Email Verification
        public bool IsEmailVerified { get; set; } = false;
        public string? EmailVerificationToken { get; set; }
        public DateTime? EmailVerificationExpiry { get; set; }

        // Google OAuth flag
        public bool IsGoogleAuth { get; set; } = false;

        // Navigation properties
        public Password? Password { get; set; }
        public ICollection<UserRole> UserRoles { get; set; }
        public ICollection<ExternalLogin> ExternalLogins { get; set; }
        public ICollection<OtpRecord> OtpRecords { get; set; }
    }
}
