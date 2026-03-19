using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartSure.PolicyService.Models
{
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
        public string PropertyType { get; set; } // Apartment, House, etc.

        public int YearBuilt { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal EstimatedValue { get; set; }

        public string SecurityFeatures { get; set; }
    }
}
