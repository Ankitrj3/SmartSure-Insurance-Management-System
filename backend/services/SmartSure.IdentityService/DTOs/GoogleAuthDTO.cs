using System.ComponentModel.DataAnnotations;

namespace IdentityService.DTOs
{
    /// <summary>
    /// Represent or implements GoogleCallbackDto.
    /// </summary>
    public class GoogleCallbackDto
    {
        [Required]
        public string Code { get; set; }
    }

    /// <summary>
    /// Represent or implements GoogleUserInfoDto.
    /// </summary>
    public class GoogleUserInfoDto
    {
        public string Sub { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        public bool EmailVerified { get; set; }
        public string Picture { get; set; }
    }
}
