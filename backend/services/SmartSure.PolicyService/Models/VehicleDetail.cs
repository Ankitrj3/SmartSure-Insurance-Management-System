using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartSure.PolicyService.Models
{
    /// <summary>
    /// Represent or implements VehicleDetail.
    /// </summary>
    public class VehicleDetail
    {
        [Key]
        public Guid VehicleDetailId { get; set; }

        [Required]
        public Guid PolicyId { get; set; }

        [ForeignKey("PolicyId")]
        public Policy Policy { get; set; }

        [Required]
        [StringLength(50)]
        public string RegistrationNumber { get; set; }

        [Required]
        [StringLength(100)]
        public string Make { get; set; }

        [Required]
        [StringLength(100)]
        public string Model { get; set; }

        public int ManufactureYear { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal EstimatedValue { get; set; }

        [Required]
        public string ChassisNumber { get; set; }

        [Required]
        public string EngineNumber { get; set; }
    }
}
