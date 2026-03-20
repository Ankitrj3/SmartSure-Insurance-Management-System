using System.ComponentModel.DataAnnotations;

namespace IdentityService.DTOs
{
    public class ForgotPasswordDTO
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; }
    }

    public class VerifyOtpDTO
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; }

        [Required(ErrorMessage = "OTP is required")]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "OTP must be 6 digits")]
        public string Otp { get; set; }
    }

    public class ResetPasswordDTO
    {
        [Required(ErrorMessage = "Reset token is required")]
        public string ResetToken { get; set; }

        [Required(ErrorMessage = "New password is required")]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters long")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z\d]).{8,}$",
            ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, one digit, and one special character.")]
        public string NewPassword { get; set; }
    }

    public class VerifyEmailDTO
    {
        [Required(ErrorMessage = "Token is required")]
        public string Token { get; set; }
    }

    public class ResendVerificationDTO
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; }
    }
}
