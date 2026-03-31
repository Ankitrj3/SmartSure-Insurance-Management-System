using System.ComponentModel.DataAnnotations;

namespace IdentityService.Models
{
    /// <summary>
    /// Represent or implements Role.
    /// </summary>
    public class Role
    {
        [Key]
        public Guid RoleId {  get; set; }
        public string RoleName { get; set; }
        public ICollection<UserRole> UserRoles { get; set; }
    }
}
