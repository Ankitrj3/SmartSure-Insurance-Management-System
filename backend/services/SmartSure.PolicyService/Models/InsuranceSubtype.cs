using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartSure.PolicyService.Models
{
    /// <summary>
    /// Represent or implements InsuranceSubtype.
    /// </summary>
    public class InsuranceSubtype
    {
        [Key]
        public Guid SubtypeId { get; set; }
        
        [Required]
        public Guid TypeId { get; set; }
        
        [ForeignKey("TypeId")]
        public InsuranceType Type { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        public string Description { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal BasePremium { get; set; }
    }
}
