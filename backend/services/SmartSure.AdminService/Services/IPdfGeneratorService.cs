namespace SmartSure.AdminService.Services
{
    /// <summary>
    /// Represent or implements IPdfGeneratorService.
    /// </summary>
    public interface IPdfGeneratorService
    {
        byte[] GenerateSalesReportPdf(SalesReportData data);
    }

    /// <summary>
    /// Represent or implements SalesReportData.
    /// </summary>
    public class SalesReportData
    {
        public string ReportType { get; set; } = "Financial";
        public DateTime DateRangeStart { get; set; }
        public DateTime DateRangeEnd { get; set; }
        public int TotalPoliciesSold { get; set; }
        public int TotalUsers { get; set; }
        public int TotalClaimsReceived { get; set; }
        public int TotalClaimsApproved { get; set; }
        public int TotalClaimsRejected { get; set; }
        public int TotalClaimsPending { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal TotalClaimsPaid { get; set; }
        public List<PolicyTypeBreakdown> PolicyBreakdown { get; set; } = new();
        public List<MonthlyTrend> MonthlyTrends { get; set; } = new();
        public string GeneratedBy { get; set; } = "";
    }

    /// <summary>
    /// Represent or implements PolicyTypeBreakdown.
    /// </summary>
    public class PolicyTypeBreakdown
    {
        public string TypeName { get; set; } = "";
        public int Count { get; set; }
        public decimal Revenue { get; set; }
    }

    /// <summary>
    /// Represent or implements MonthlyTrend.
    /// </summary>
    public class MonthlyTrend
    {
        public string Month { get; set; } = "";
        public int PoliciesSold { get; set; }
        public int ClaimsReceived { get; set; }
        public decimal Revenue { get; set; }
    }
}
