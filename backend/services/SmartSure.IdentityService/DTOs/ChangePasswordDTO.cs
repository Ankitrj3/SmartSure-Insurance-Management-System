using System.ComponentModel.DataAnnotations;

namespace IdentityService.DTOs
{
    /// <summary>
    /// Represent or implements ChangePasswordDTO.
    /// </summary>
    public class ChangePasswordDTO
    {
        [Required(ErrorMessage = "Current Password is required")]
        public string OldPassword { get; set; }

        [Required(ErrorMessage = "New Password is required")]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters long")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z\d]).{8,}$", 
            ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, one digit, and one special character.")]
        public string NewPassword { get; set; }
    }
}
