using System.ComponentModel.DataAnnotations;
using SmartSure.Shared.Contracts.Utilities;

namespace SmartSure.AdminService.DTOs
{
    /// <summary>
    /// Represent or implements ClaimReviewDTO.
    /// </summary>
    public class ClaimReviewDTO
    {
        public string? Notes { get; set; }
    }

    /// <summary>
    /// Represent or implements ClaimApprovalDTO.
    /// </summary>
    public class ClaimApprovalDTO
    {
        [Range(0.01, double.MaxValue)]
        public decimal? ApprovedAmount { get; set; }
        public string? Notes { get; set; }
    }

    /// <summary>
    /// Represent or implements ClaimRejectionDTO.
    /// </summary>
    public class ClaimRejectionDTO
    {
        [Required(ErrorMessage = "Rejection reason is required")]
        [StringLength(500)]
        public string Reason { get; set; } = "";
    }

    /// <summary>
    /// Represent or implements ReportRequestDTO.
    /// </summary>
    public class ReportRequestDTO
    {
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = "";

        [Required]
        [StringLength(50)]
        public string ReportType { get; set; } = "";

        [StringLength(50)]
        public string Format { get; set; } = "PDF";

        [Required]
        public DateTime DateRangeStart { get; set; }

        [Required]
        public DateTime DateRangeEnd { get; set; }
    }

    /// <summary>
    /// Represent or implements ReportResponseDTO.
    /// </summary>
    public class ReportResponseDTO
    {
        public Guid ReportId { get; set; }
        public string FormattedReportId => ReportId.FormatApiId("REP");
        public string Title { get; set; } = "";
        public string ReportType { get; set; } = "";
        public string Format { get; set; } = "";
        public string? Content { get; set; }
        public Guid GeneratedBy { get; set; }
        public DateTime DateRangeStart { get; set; }
        public DateTime DateRangeEnd { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// Represent or implements DashboardStatsDTO.
    /// </summary>
    public class DashboardStatsDTO
    {
        public int TotalUsers { get; set; }
        public int TotalPolicies { get; set; }
        public int TotalClaims { get; set; }
        public int PendingClaims { get; set; }
        public int ApprovedClaims { get; set; }
        public int RejectedClaims { get; set; }
        public int ActivePolicies { get; set; }
        public decimal TotalRevenue { get; set; }
    }

    /// <summary>
    /// Represent or implements AuditLogDTO.
    /// </summary>
    public class AuditLogDTO
    {
        public Guid Id { get; set; }
        public string Action { get; set; } = "";
        public string EntityType { get; set; } = "";
        public Guid? EntityId { get; set; }
        public string FormattedEntityId => EntityId?.FormatApiId("LOG") ?? "";
        public Guid? ActorId { get; set; }
        public string FormattedActorId => ActorId?.FormatApiId("SSUSER") ?? "";
        public string? Details { get; set; }
        public DateTime Timestamp { get; set; }
    }

    /// <summary>
    /// Represent or implements AdminUserDTO.
    /// </summary>
    public class AdminUserDTO
    {
        public Guid UserId { get; set; }
        public string FormattedUserId => UserId.FormatApiId("SSUSER");
        public string FullName { get; set; } = "";
        public string Email { get; set; } = "";
        public List<string> Roles { get; set; } = new();
        public bool IsActive { get; set; } = true;
    }
}
