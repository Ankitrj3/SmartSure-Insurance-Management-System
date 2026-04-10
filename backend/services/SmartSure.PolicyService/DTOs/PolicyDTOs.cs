using System.ComponentModel.DataAnnotations;
using SmartSure.Shared.Contracts.Utilities;

namespace SmartSure.PolicyService.DTOs
{
    /// <summary>
    /// Represent or implements PolicyDTO.
    /// </summary>
    public class PolicyDTO
    {
        public Guid PolicyId { get; set; }
        public string FormattedPolicyId => PolicyId.FormatApiId("POL");
        
        public Guid UserId { get; set; }
        public string FormattedUserId => UserId.FormatApiId("SSUSER");
        
        public Guid SubtypeId { get; set; }
        public string FormattedSubtypeId => SubtypeId.FormatApiId("PLAN");
        
        public string? SubtypeName { get; set; }
        public string? TypeName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal PremiumAmount { get; set; }
        public decimal InsuredDeclaredValue { get; set; }
        public string? Status { get; set; }
        public string? NomineeName { get; set; }
        public string? NomineeRelation { get; set; }
    }

    /// <summary>
    /// Represent or implements CreatePolicyDTO.
    /// </summary>
    public class CreatePolicyDTO
    {
        [Required(ErrorMessage = "SubtypeId is required")]
        public Guid SubtypeId { get; set; }

        [Required(ErrorMessage = "Duration is required (months)")]
        [Range(1, 120, ErrorMessage = "Duration must be between 1 and 120 months")]
        public int Duration { get; set; }

        // Home detail fields (wizard can submit if it's a home policy)
        public PolicyHomeDetailDTO? HomeDetail { get; set; }

        // Vehicle detail fields (wizard can submit if it's a vehicle policy)
        public PolicyVehicleDetailDTO? VehicleDetail { get; set; }

        public string? CouponCode { get; set; }

        public string? NomineeName { get; set; }
        public string? NomineeRelation { get; set; }
    }

    /// <summary>
    /// Home detail for inline policy creation (no PolicyId needed)
    /// </summary>
    public class PolicyHomeDetailDTO
    {
        [Required(ErrorMessage = "Address is required")]
        [StringLength(500)]
        public string Address { get; set; }

        [Required(ErrorMessage = "PropertyType is required")]
        public string PropertyType { get; set; }

        public int YearBuilt { get; set; }

        /// <summary>Area of the house/flat in sq.ft (used in IDV = Area × Cost/sqft).</summary>
        [Required(ErrorMessage = "Area in sq.ft is required")]
        [Range(1, 1000000, ErrorMessage = "Area must be a positive number")]
        public decimal AreaSqFt { get; set; }

        /// <summary>Reconstruction cost per sq.ft in INR.</summary>
        [Required(ErrorMessage = "Construction cost per sq.ft is required")]
        [Range(1, double.MaxValue, ErrorMessage = "Construction cost must be positive")]
        public decimal ConstructionCostPerSqFt { get; set; }

        public string? SecurityFeatures { get; set; }
    }

    /// <summary>
    /// Vehicle detail for inline policy creation (no PolicyId needed)
    /// </summary>
    public class PolicyVehicleDetailDTO
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

    /// <summary>
    /// Home detail for separate save endpoint (requires PolicyId)
    /// </summary>
    public class CreateHomeDetailDTO
    {
        public Guid? PolicyId { get; set; }

        [Required(ErrorMessage = "Address is required")]
        [StringLength(500)]
        public string Address { get; set; }

        [Required(ErrorMessage = "PropertyType is required")]
        public string PropertyType { get; set; }

        public int YearBuilt { get; set; }

        /// <summary>Area of the house/flat in sq.ft.</summary>
        [Required(ErrorMessage = "Area in sq.ft is required")]
        [Range(1, 1000000)]
        public decimal AreaSqFt { get; set; }

        /// <summary>Reconstruction cost per sq.ft in INR.</summary>
        [Required(ErrorMessage = "Construction cost per sq.ft is required")]
        [Range(1, double.MaxValue)]
        public decimal ConstructionCostPerSqFt { get; set; }

        /// <summary>Kept for backwards compat; will be auto-set to AreaSqFt × ConstructionCostPerSqFt.</summary>
        public decimal EstimatedValue { get; set; }

        public string? SecurityFeatures { get; set; }
    }

    /// <summary>
    /// Vehicle detail for separate save endpoint (requires PolicyId)
    /// </summary>
    public class CreateVehicleDetailDTO
    {
        public Guid? PolicyId { get; set; }

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

    /// <summary>
    /// Returned after IDV/Insurance value calculation before purchase confirmation.
    /// </summary>
    public class PolicyQuoteDTO
    {
        public Guid SubtypeId { get; set; }
        public string SubtypeName { get; set; }
        public string TypeName { get; set; }
        public int Duration { get; set; }
        public decimal InsuredDeclaredValue { get; set; }
        public decimal PremiumAmount { get; set; }
        public string Breakdown { get; set; }
    }

    /// <summary>
    /// Represent or implements PolicyDetailDTO.
    /// </summary>
    public class PolicyDetailDTO
    {
        public Guid PolicyId { get; set; }
        public string? TermsAndConditions { get; set; }
        public string? Inclusions { get; set; }
        public string? Exclusions { get; set; }
    }

    /// <summary>
    /// Represent or implements SavePolicyDetailDTO.
    /// </summary>
    public class SavePolicyDetailDTO
    {
        [Required]
        public string TermsAndConditions { get; set; }
        public string? Inclusions { get; set; }
        public string? Exclusions { get; set; }
    }
}
