using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartSure.ClaimsService.Models
{
    /// <summary>
    /// Represent or implements Claim.
    /// </summary>
    public class Claim
    {
        [Key]
        public Guid ClaimId { get; set; }

        [Required]
        public Guid PolicyId { get; set; }

        [Required]
        public Guid UserId { get; set; }

        [Required]
        [StringLength(500)]
        public string Description { get; set; } = "";

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Draft"; // Draft, Submitted, UnderReview, Approved, Rejected, Closed

        [Column(TypeName = "decimal(18,2)")]
        public decimal ClaimAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? ApprovedAmount { get; set; }

        public string? RejectionReason { get; set; }

        public string? ClaimType { get; set; }
        
        public bool? IsCompletelyDamaged { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public ICollection<ClaimDocument> Documents { get; set; } = new List<ClaimDocument>();
        public ICollection<ClaimStatusHistory> StatusHistory { get; set; } = new List<ClaimStatusHistory>();
    }
}
