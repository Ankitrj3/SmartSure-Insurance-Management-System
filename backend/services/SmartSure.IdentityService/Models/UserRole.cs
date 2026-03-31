using System.ComponentModel.DataAnnotations;

namespace IdentityService.Models
{
    /// <summary>
    /// Represent or implements UserRole.
    /// </summary>
    public class UserRole
    {
        [Key]
        public Guid Id {  get; set; }
        public Guid UserId { get; set; }
        public Guid RoleId { get; set; }
        public User User { get; set; }
        public Role Role { get; set; }
    }
}
