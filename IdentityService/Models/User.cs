using System.ComponentModel.DataAnnotations;

namespace IdentityService.Models
{
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
        [Required]
        public Password Password { get; set; }
        public ICollection<UserRole> UserRoles { get; set; }
    }
}
