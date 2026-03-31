using System.ComponentModel.DataAnnotations;

namespace SmartSure.AdminService.Models
{
    /// <summary>
    /// Represent or implements Report.
    /// </summary>
    public class Report
    {
        [Key]
        public Guid ReportId { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = "";

        [Required]
        [StringLength(50)]
        public string ReportType { get; set; } = "";  // "PolicySummary", "ClaimsSummary", "Revenue", "UserActivity"

        [Required]
        [StringLength(50)]
        public string Format { get; set; } = "PDF"; // "PDF", "CSV"

        public string? Content { get; set; }  // JSON payload or file path

        public Guid GeneratedBy { get; set; }  // Admin UserId

        public DateTime DateRangeStart { get; set; }
        public DateTime DateRangeEnd { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
