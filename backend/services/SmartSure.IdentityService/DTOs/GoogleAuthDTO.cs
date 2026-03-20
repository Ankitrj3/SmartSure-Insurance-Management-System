using System.ComponentModel.DataAnnotations;

namespace IdentityService.DTOs
{
    public class GoogleCallbackDto
    {
        [Required]
        public string Code { get; set; }
    }

    public class GoogleUserInfoDto
    {
        public string Sub { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        public bool EmailVerified { get; set; }
        public string Picture { get; set; }
    }
}
