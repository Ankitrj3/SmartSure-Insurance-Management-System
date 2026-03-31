using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartSure.ClaimsService.Models
{
    /// <summary>
    /// Represent or implements ClaimStatusHistory.
    /// </summary>
    public class ClaimStatusHistory
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid ClaimId { get; set; }

        [Required]
        [StringLength(50)]
        public string OldStatus { get; set; } = "";

        [Required]
        [StringLength(50)]
        public string NewStatus { get; set; } = "";

        [StringLength(500)]
        public string? Notes { get; set; }

        [Required]
        public string ChangedBy { get; set; } = ""; // UserId of the person who changed

        public DateTime ChangedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        [ForeignKey("ClaimId")]
        public Claim Claim { get; set; }
    }
}
