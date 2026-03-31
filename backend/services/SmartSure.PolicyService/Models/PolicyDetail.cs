using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartSure.PolicyService.Models
{
    /// <summary>
    /// Represent or implements PolicyDetail.
    /// </summary>
    public class PolicyDetail
    {
        [Key]
        public Guid DocumentId { get; set; }

        [Required]
        public Guid PolicyId { get; set; }

        [ForeignKey("PolicyId")]
        public Policy Policy { get; set; }

        [Required]
        public string TermsAndConditions { get; set; }

        public string Inclusions { get; set; }
        
        public string Exclusions { get; set; }
    }
}
