using System.ComponentModel.DataAnnotations;

namespace IdentityService.Models
{
    /// <summary>
    /// Represent or implements OtpRecord.
    /// </summary>
    public class OtpRecord
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid UserId { get; set; }

        [Required]
        public string Email { get; set; }

        [Required]
        public string Otp { get; set; }

        public DateTime ExpirationTime { get; set; }

        public int Attempts { get; set; } = 0;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public User User { get; set; }
    }
}
