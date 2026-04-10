using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartSure.PolicyService.Models
{
    /// <summary>
    /// Represent or implements HomeDetail.
    /// </summary>
    public class HomeDetail
    {
        [Key]
        public Guid HomeDetailId { get; set; }

        [Required]
        public Guid PolicyId { get; set; }

        [ForeignKey("PolicyId")]
        public Policy Policy { get; set; }

        [Required]
        [StringLength(500)]
        public string Address { get; set; }

        [Required]
        public string PropertyType { get; set; } // Apartment, House, Villa, etc.

        public int YearBuilt { get; set; }

        /// <summary>Area of the house/flat in square feet.</summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal AreaSqFt { get; set; }

        /// <summary>Current reconstruction / construction cost per sq.ft in INR.</summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal ConstructionCostPerSqFt { get; set; }

        /// <summary>Legacy / computed field: AreaSqFt × ConstructionCostPerSqFt.</summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal EstimatedValue { get; set; }

        public string SecurityFeatures { get; set; }
    }
}
