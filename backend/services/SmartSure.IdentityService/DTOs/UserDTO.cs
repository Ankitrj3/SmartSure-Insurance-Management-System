using SmartSure.Shared.Contracts.Utilities;

namespace IdentityService.DTOs
{
    /// <summary>
    /// Represent or implements UserDTO.
    /// </summary>
    public class UserDTO
    {
        public Guid UserId { get; set; }
        public string FormattedUserId => UserId.FormatApiId("SSUSER");
        public string Email { get; set; }
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public List<string> Roles { get; set; } = new();
    }
}
