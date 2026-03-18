using System.ComponentModel.DataAnnotations;

namespace IdentityService.Models
{
    public class User
    {
        [Key]
        public Guid UserId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public Password Password { get; set; }
        public ICollection<UserRole> UserRoles { get; set; }
    }
}
