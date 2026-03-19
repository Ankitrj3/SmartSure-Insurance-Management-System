using System.ComponentModel.DataAnnotations;

namespace SmartSure.PolicyService.DTOs
{
    public class PolicyDTO
    {
        public Guid PolicyId { get; set; }
        public Guid UserId { get; set; }
        public Guid SubtypeId { get; set; }
        public string SubtypeName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal PremiumAmount { get; set; }
        public string Status { get; set; }
    }

    public class CreatePolicyDTO
    {
        [Required(ErrorMessage = "SubtypeId is required")]
        public Guid SubtypeId { get; set; }

        [Required(ErrorMessage = "Duration is required (months)")]
        [Range(1, 120, ErrorMessage = "Duration must be between 1 and 120 months")]
        public int Duration { get; set; }

        // Home detail fields (wizard can submit if it's a home policy)
        public CreateHomeDetailDTO HomeDetail { get; set; }

        // Vehicle detail fields (wizard can submit if it's a vehicle policy)
        public CreateVehicleDetailDTO VehicleDetail { get; set; }
    }

    public class CreateHomeDetailDTO
    {
        [Required(ErrorMessage = "Address is required")]
        [StringLength(500)]
        public string Address { get; set; }

        [Required(ErrorMessage = "PropertyType is required")]
        public string PropertyType { get; set; }

        public int YearBuilt { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal EstimatedValue { get; set; }

        public string SecurityFeatures { get; set; }
    }

    public class CreateVehicleDetailDTO
    {
        [Required(ErrorMessage = "RegistrationNumber is required")]
        [StringLength(50)]
        public string RegistrationNumber { get; set; }

        [Required(ErrorMessage = "Make is required")]
        public string Make { get; set; }

        [Required(ErrorMessage = "Model is required")]
        public string Model { get; set; }

        public int ManufactureYear { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal EstimatedValue { get; set; }

        [Required]
        public string ChassisNumber { get; set; }

        [Required]
        public string EngineNumber { get; set; }
    }

    public class PolicyDetailDTO
    {
        public Guid PolicyId { get; set; }
        public string TermsAndConditions { get; set; }
        public string Inclusions { get; set; }
        public string Exclusions { get; set; }
    }

    public class SavePolicyDetailDTO
    {
        [Required]
        public string TermsAndConditions { get; set; }
        public string Inclusions { get; set; }
        public string Exclusions { get; set; }
    }
}
