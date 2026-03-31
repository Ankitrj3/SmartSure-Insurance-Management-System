using System.ComponentModel.DataAnnotations;

namespace SmartSure.PolicyService.DTOs
{
    /// <summary>
    /// Represent or implements InsuranceSubtypeDTO.
    /// </summary>
    public class InsuranceSubtypeDTO
    {
        public Guid SubtypeId { get; set; }
        public Guid TypeId { get; set; }
        public string? TypeName { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal BasePremium { get; set; }
    }

    /// <summary>
    /// Represent or implements CreateInsuranceSubtypeDTO.
    /// </summary>
    public class CreateInsuranceSubtypeDTO
    {
        [Required(ErrorMessage = "TypeId is required")]
        public Guid TypeId { get; set; }

        [Required(ErrorMessage = "Name is required")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        public string Name { get; set; }

        public string Description { get; set; }

        [Required(ErrorMessage = "BasePremium is required")]
        [Range(0, double.MaxValue, ErrorMessage = "BasePremium must be a positive value")]
        public decimal BasePremium { get; set; }
    }

    /// <summary>
    /// Represent or implements UpdateInsuranceSubtypeDTO.
    /// </summary>
    public class UpdateInsuranceSubtypeDTO
    {
        [Required(ErrorMessage = "Name is required")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        public string Name { get; set; }

        public string Description { get; set; }

        [Required(ErrorMessage = "BasePremium is required")]
        [Range(0, double.MaxValue, ErrorMessage = "BasePremium must be a positive value")]
        public decimal BasePremium { get; set; }
    }
}
