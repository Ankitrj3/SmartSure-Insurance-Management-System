using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SmartSure.AdminService.Controllers
{
    [ApiController]
    [Route("admin/users")]
    [Authorize(Roles = "Admin")]
    public class AdminUsersController : ControllerBase
    {
        private readonly ILogger<AdminUsersController> _logger;

        public AdminUsersController(ILogger<AdminUsersController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IActionResult GetUsers()
        {
            // In production, this would call Identity Service via HTTP or shared DB
            _logger.LogInformation("Admin requested user list");
            return Ok(new { message = "User list would be fetched from Identity Service" });
        }

        [HttpGet("{userId}")]
        public IActionResult GetUser(Guid userId)
        {
            _logger.LogInformation("Admin requested user detail for {UserId}", userId);
            return Ok(new { message = $"User detail for {userId} would be fetched from Identity Service" });
        }

        [HttpDelete("{userId}")]
        public IActionResult DeactivateUser(Guid userId)
        {
            _logger.LogInformation("Admin deactivated user {UserId}", userId);
            return Ok(new { message = $"User {userId} deactivated" });
        }
    }
}
