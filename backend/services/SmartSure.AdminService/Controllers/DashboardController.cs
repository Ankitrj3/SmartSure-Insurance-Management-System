using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SmartSure.AdminService.Controllers
{
    [ApiController]
    [Route("admin/dashboard")]
    [Authorize(Roles = "Admin")]
    public class DashboardController : ControllerBase
    {
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(ILogger<DashboardController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IActionResult GetDashboardStats()
        {
            // In production, this would aggregate data from other services via HTTP calls or a shared read DB
            // For now return placeholder structure
            var stats = new
            {
                totalUsers = 0,
                totalPolicies = 0,
                totalClaims = 0,
                pendingClaims = 0,
                approvedClaims = 0,
                rejectedClaims = 0,
                activePolicies = 0,
                totalRevenue = 0m
            };

            _logger.LogInformation("Dashboard stats requested");

            return Ok(stats);
        }
    }
}
