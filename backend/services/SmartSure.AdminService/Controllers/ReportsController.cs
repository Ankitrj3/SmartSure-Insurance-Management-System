using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartSure.AdminService.DTOs;
using SmartSure.AdminService.Services;
using System.Security.Claims;
using SmartSure.Shared.Contracts.Exceptions;

namespace SmartSure.AdminService.Controllers
{
    /// <summary>
    /// Represent or implements ReportsController.
    /// </summary>
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

        /// <summary>
        /// Performs the GetReports operation.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetReports()
        {
            var reports = await _reportService.GetReportsAsync();
            return Ok(reports);
        }

        /// <summary>
        /// Performs the GenerateReport operation.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> GenerateReport([FromBody] ReportRequestDTO dto)
        {
            try
            {
                var adminId = GetAdminId();
                string token = Request.Headers["Authorization"].ToString();
                var report = await _reportService.GenerateReportAsync(adminId, dto, token);
                return CreatedAtAction(nameof(GetReport), new { reportId = report.ReportId }, report);
            }
            catch (SmartSureException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new BusinessRuleException($"Failed to generate report: {ex.Message}");
            }
        }

        /// <summary>
        /// Performs the GeneratePdfReport operation.
        /// </summary>
        [HttpPost("pdf")]
        public async Task<IActionResult> GeneratePdfReport([FromBody] ReportRequestDTO dto)
        {
            try
            {
                var adminId = GetAdminId();
                string token = Request.Headers["Authorization"].ToString();
                
                var pdfBytes = await _reportService.GeneratePdfReportAsync(adminId, dto, token);
                
                var fileName = $"SmartSure_Report_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
                return File(pdfBytes, "application/pdf", fileName);
            }
            catch (SmartSureException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new BusinessRuleException($"Failed to generate PDF report: {ex.Message}");
            }
        }

        /// <summary>
        /// Performs the GetReport operation.
        /// </summary>
        [HttpGet("{reportId}")]
        public async Task<IActionResult> GetReport(Guid reportId)
        {
            var report = await _reportService.GetReportByIdAsync(reportId);
            if (report == null) return NotFound(new { message = "Report not found" });
            return Ok(report);
        }

        /// <summary>
        /// Performs the DownloadReport operation.
        /// </summary>
        [HttpGet("{reportId}/download")]
        public async Task<IActionResult> DownloadReport(Guid reportId)
        {
            var report = await _reportService.GetReportByIdAsync(reportId);
            if (report == null) return NotFound(new { message = "Report not found" });
            
            byte[] bytes;
            // First try Base64 if it's there
            if (!string.IsNullOrEmpty(report.Content) && !report.Content.StartsWith("PDF Report") && !report.Content.StartsWith("{"))
            {
                try
                {
                    bytes = Convert.FromBase64String(report.Content);
                    return File(bytes, "application/pdf", $"Report_{report.Title.Replace(" ", "_")}.pdf");
                }
                catch { } // Ignore and fallback
            }

            // Fallback for old reports
            string token = Request.Headers["Authorization"].ToString();
            bytes = await _reportService.RegeneratePdfAsync(reportId, token);
            return File(bytes, "application/pdf", $"Report_{report.Title.Replace(" ", "_")}.pdf");
        }

        /// <summary>
        /// Performs the DeleteReport operation.
        /// </summary>
        [HttpDelete("{reportId}")]
        public async Task<IActionResult> DeleteReport(Guid reportId)
        {
            var success = await _reportService.DeleteReportAsync(reportId);
            if (!success) return NotFound(new { message = "Report not found or could not be deleted" });
            return NoContent();
        }
    }
}
