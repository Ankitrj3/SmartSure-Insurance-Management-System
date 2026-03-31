using System.ComponentModel.DataAnnotations;

namespace IdentityService.Models
{
    /// <summary>
    /// Represent or implements ExternalLogin.
    /// </summary>
    public class ExternalLogin
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid UserId { get; set; }

        [Required]
        [StringLength(50)]
        public string Provider { get; set; }  // "Google"

        [Required]
        [StringLength(256)]
        public string ProviderKey { get; set; }  // Google sub ID

        [EmailAddress]
        public string Email { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public User User { get; set; }
    }
}
