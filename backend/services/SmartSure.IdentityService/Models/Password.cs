using System.ComponentModel.DataAnnotations;

namespace IdentityService.Models
{
    /// <summary>
    /// Represent or implements Password.
    /// </summary>
    public class Password
    {
        [Key]
        public Guid PassId { get; set; }
        public Guid UserId { get; set; }
        public string PasswordHash { get; set; }
        public User User { get; set; }
    }
}
