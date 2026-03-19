using System.ComponentModel.DataAnnotations;

namespace SmartSure.PolicyService.Models
{
    public class InsuranceType
    {
        [Key]
        public Guid TypeId { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        public string Description { get; set; }

        public ICollection<InsuranceSubtype> Subtypes { get; set; }
    }
}
