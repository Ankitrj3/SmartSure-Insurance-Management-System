using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartSure.PolicyService.Models
{
    /// <summary>
    /// Represent or implements Policy.
    /// </summary>
    public class Policy
    {
        [Key]
        public Guid PolicyId { get; set; }

        [Required]
        public Guid UserId { get; set; }

        [Required]
        public Guid SubtypeId { get; set; }

        [ForeignKey("SubtypeId")]
        public InsuranceSubtype Subtype { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal PremiumAmount { get; set; }

        /// <summary>
        /// Insured Declared Value – calculated by the system based on asset details.
        /// For Vehicle: depreciation-based IDV. For Home: rebuild-cost-based insurance value.
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal InsuredDeclaredValue { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; } // "Pending", "Active", "Cancelled", "Expired"

        /// <summary>
        /// Number of approved claims made against this policy
        /// </summary>
        public int ApprovedClaimsCount { get; set; } = 0;

        /// <summary>
        /// Indicates if the policy has been terminated due to theft/total loss
        /// </summary>
        public bool IsTerminated { get; set; } = false;

        /// <summary>
        /// Invoice generation timestamp
        /// </summary>
        public DateTime? InvoiceGeneratedAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public PolicyDetail PolicyDetail { get; set; }
        public HomeDetail HomeDetail { get; set; }
        public VehicleDetail VehicleDetail { get; set; }
        public ICollection<Payment> Payments { get; set; }

        // Nominee details for the policy (optional)
        [StringLength(100)]
        public string? NomineeName { get; set; }
        
        [StringLength(50)]
        public string? NomineeRelation { get; set; }
    }
}
