using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartSure.ClaimsService.Models
{
    /// <summary>
    /// Represent or implements ClaimDocument.
    /// </summary>
    public class ClaimDocument
    {
        [Key]
        public Guid DocumentId { get; set; }

        [Required]
        public Guid ClaimId { get; set; }

        [Required]
        [StringLength(255)]
        public string FileName { get; set; } = "";

        [Required]
        [StringLength(1000)]
        public string FileUrl { get; set; } = "";  // Mega.nz URL

        [StringLength(100)]
        public string ContentType { get; set; } = "";

        public long FileSize { get; set; }

        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        [ForeignKey("ClaimId")]
        public Claim Claim { get; set; }
    }
}
