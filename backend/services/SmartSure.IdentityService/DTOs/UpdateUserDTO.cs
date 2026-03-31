using System.ComponentModel.DataAnnotations;

namespace IdentityService.DTOs
{
    /// <summary>
    /// Represent or implements UpdateUserDTO.
    /// </summary>
    public class UpdateUserDTO
    {
        [Required(ErrorMessage = "Full Name is required")]
        [StringLength(100, ErrorMessage = "Full Name cannot exceed 100 characters")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Phone Number is required")]
        [Phone(ErrorMessage = "Invalid phone number format")]
        public string PhoneNumber { get; set; }

        [StringLength(500, ErrorMessage = "Address cannot exceed 500 characters")]
        public string Address { get; set; }
    }
}
