using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartSure.AdminService.Services;

namespace SmartSure.AdminService.Controllers
{
    [ApiController]
    [Route("admin/audit-logs")]
    [Authorize(Roles = "Admin")]
    /// <summary>
    /// Represent or implements AuditLogsController.
    /// </summary>
    public class AuditLogsController : ControllerBase
    {
        private readonly IAuditService _auditService;

        public AuditLogsController(IAuditService auditService)
        {
            _auditService = auditService;
        }

        [HttpGet]
        /// <summary>
        /// Performs the GetAuditLogs operation.
        /// </summary>
        public async Task<IActionResult> GetAuditLogs([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 20;

            var logs = await _auditService.GetAuditLogsAsync(page, pageSize);
            var totalCount = await _auditService.GetTotalAuditLogsCountAsync();

            return Ok(new
            {
                items = logs,
                totalCount,
                page,
                pageSize,
                totalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            });
        }
    }
}
