using System.ComponentModel.DataAnnotations;

namespace SmartSure.ClaimsService.DTOs
{
    public class CreateClaimDTO
    {
        [Required(ErrorMessage = "PolicyId is required")]
        public Guid PolicyId { get; set; }

        [Required(ErrorMessage = "Description is required")]
        [StringLength(500)]
        public string Description { get; set; } = "";

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Claim amount must be positive")]
        public decimal ClaimAmount { get; set; }
    }

    public class UpdateClaimDTO
    {
        [StringLength(500)]
        public string? Description { get; set; }

        [Range(0.01, double.MaxValue)]
        public decimal? ClaimAmount { get; set; }
    }

    public class ClaimResponseDTO
    {
        public Guid ClaimId { get; set; }
        public Guid PolicyId { get; set; }
        public Guid UserId { get; set; }
        public string Description { get; set; } = "";
        public string Status { get; set; } = "";
        public decimal ClaimAmount { get; set; }
        public decimal? ApprovedAmount { get; set; }
        public string? RejectionReason { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<DocumentResponseDTO> Documents { get; set; } = new();
    }

    public class DocumentUploadDTO
    {
        [Required(ErrorMessage = "FileName is required")]
        [StringLength(255)]
        public string FileName { get; set; } = "";

        [Required(ErrorMessage = "FileUrl is required")]
        [StringLength(1000)]
        public string FileUrl { get; set; } = "";  // Mega.nz URL

        [StringLength(100)]
        public string ContentType { get; set; } = "";

        public long FileSize { get; set; }
    }

    public class DocumentResponseDTO
    {
        public Guid DocumentId { get; set; }
        public Guid ClaimId { get; set; }
        public string FileName { get; set; } = "";
        public string FileUrl { get; set; } = "";
        public string ContentType { get; set; } = "";
        public long FileSize { get; set; }
        public DateTime UploadedAt { get; set; }
    }

    public class ClaimStatusHistoryDTO
    {
        public Guid Id { get; set; }
        public string OldStatus { get; set; } = "";
        public string NewStatus { get; set; } = "";
        public string? Notes { get; set; }
        public string ChangedBy { get; set; } = "";
        public DateTime ChangedAt { get; set; }
    }

    public class ApproveClaimDTO
    {
        public decimal ApprovedAmount { get; set; }
        public string? Notes { get; set; }
    }

    public class RejectClaimDTO
    {
        [Required]
        public string Reason { get; set; } = "";
    }
}
