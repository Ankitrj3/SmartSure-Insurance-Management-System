using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartSure.AdminService.DTOs;
using SmartSure.AdminService.Services;
using System.Security.Claims;

namespace SmartSure.AdminService.Controllers
{
    [ApiController]
    [Route("admin/reports")]
    [Authorize(Roles = "Admin")]
    public class ReportsController : ControllerBase
    {
        private readonly IReportService _reportService;

        public ReportsController(IReportService reportService)
        {
            _reportService = reportService;
        }

        private Guid GetAdminId() =>
            Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        [HttpGet]
        public async Task<IActionResult> GetReports()
        {
            var reports = await _reportService.GetReportsAsync();
            return Ok(reports);
        }

        [HttpPost]
        public async Task<IActionResult> GenerateReport([FromBody] ReportRequestDTO dto)
        {
            var adminId = GetAdminId();
            var report = await _reportService.GenerateReportAsync(adminId, dto);
            return CreatedAtAction(nameof(GetReport), new { reportId = report.ReportId }, report);
        }

        [HttpGet("{reportId}")]
        public async Task<IActionResult> GetReport(Guid reportId)
        {
            var report = await _reportService.GetReportByIdAsync(reportId);
            if (report == null) return NotFound(new { message = "Report not found" });
            return Ok(report);
        }
    }
}
