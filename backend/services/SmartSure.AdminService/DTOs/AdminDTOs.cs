using System.ComponentModel.DataAnnotations;

namespace SmartSure.AdminService.DTOs
{
    public class ClaimReviewDTO
    {
        public string? Notes { get; set; }
    }

    public class ClaimApprovalDTO
    {
        [Range(0.01, double.MaxValue)]
        public decimal? ApprovedAmount { get; set; }
        public string? Notes { get; set; }
    }

    public class ClaimRejectionDTO
    {
        [Required(ErrorMessage = "Rejection reason is required")]
        [StringLength(500)]
        public string Reason { get; set; } = "";
    }

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

    public class ReportResponseDTO
    {
        public Guid ReportId { get; set; }
        public string Title { get; set; } = "";
        public string ReportType { get; set; } = "";
        public string Format { get; set; } = "";
        public string? Content { get; set; }
        public Guid GeneratedBy { get; set; }
        public DateTime DateRangeStart { get; set; }
        public DateTime DateRangeEnd { get; set; }
        public DateTime CreatedAt { get; set; }
    }

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

    public class AuditLogDTO
    {
        public Guid Id { get; set; }
        public string Action { get; set; } = "";
        public string EntityType { get; set; } = "";
        public Guid? EntityId { get; set; }
        public Guid? ActorId { get; set; }
        public string? Details { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public class AdminUserDTO
    {
        public Guid UserId { get; set; }
        public string FullName { get; set; } = "";
        public string Email { get; set; } = "";
        public List<string> Roles { get; set; } = new();
        public bool IsActive { get; set; } = true;
    }
}
