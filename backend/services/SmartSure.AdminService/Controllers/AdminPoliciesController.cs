using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SmartSure.AdminService.Controllers
{
    [ApiController]
    [Route("admin/policies")]
    [Authorize(Roles = "Admin")]
    public class AdminPoliciesController : ControllerBase
    {
        private readonly ILogger<AdminPoliciesController> _logger;

        public AdminPoliciesController(ILogger<AdminPoliciesController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IActionResult GetAllPolicies()
        {
            // In production, this would call Policy Service via HTTP or shared DB
            _logger.LogInformation("Admin requested all policies");
            return Ok(new { message = "All policies would be fetched from Policy Service" });
        }
    }
}
