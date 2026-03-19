using System.ComponentModel.DataAnnotations;

namespace SmartSure.PolicyService.DTOs
{
    public class InsuranceTypeDTO
    {
        public Guid TypeId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }

    public class CreateInsuranceTypeDTO
    {
        [Required(ErrorMessage = "Name is required")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        public string Name { get; set; }

        public string Description { get; set; }
    }

    public class UpdateInsuranceTypeDTO
    {
        [Required(ErrorMessage = "Name is required")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        public string Name { get; set; }

        public string Description { get; set; }
    }
}
