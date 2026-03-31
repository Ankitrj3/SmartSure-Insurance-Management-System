using System.ComponentModel.DataAnnotations;

namespace IdentityService.DTOs
{
    /// <summary>
    /// Represent or implements LoginDTO.
    /// </summary>
    public class LoginDTO
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; }
    }
}
