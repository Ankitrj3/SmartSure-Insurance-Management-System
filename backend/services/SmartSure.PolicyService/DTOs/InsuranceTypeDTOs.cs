using System.ComponentModel.DataAnnotations;

namespace SmartSure.PolicyService.DTOs
{
    /// <summary>
    /// Represent or implements InsuranceTypeDTO.
    /// </summary>
    public class InsuranceTypeDTO
    {
        public Guid TypeId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }

    /// <summary>
    /// Represent or implements CreateInsuranceTypeDTO.
    /// </summary>
    public class CreateInsuranceTypeDTO
    {
        [Required(ErrorMessage = "Name is required")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        public string Name { get; set; }

        public string Description { get; set; }
    }

    /// <summary>
    /// Represent or implements UpdateInsuranceTypeDTO.
    /// </summary>
    public class UpdateInsuranceTypeDTO
    {
        [Required(ErrorMessage = "Name is required")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        public string Name { get; set; }

        public string Description { get; set; }
    }
}
