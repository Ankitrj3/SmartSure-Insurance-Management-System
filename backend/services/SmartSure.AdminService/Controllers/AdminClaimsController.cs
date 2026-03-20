using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartSure.AdminService.DTOs;
using SmartSure.AdminService.Services;
using SmartSure.Shared.Contracts.Events;
using System.Security.Claims;

namespace SmartSure.AdminService.Controllers
{
    [ApiController]
    [Route("admin/claims")]
    [Authorize(Roles = "Admin")]
    public class AdminClaimsController : ControllerBase
    {
        private readonly IAuditService _auditService;
        private readonly IBus _bus;
        private readonly ILogger<AdminClaimsController> _logger;

        public AdminClaimsController(IAuditService auditService, IBus bus, ILogger<AdminClaimsController> logger)
        {
            _auditService = auditService;
            _bus = bus;
            _logger = logger;
        }

        private Guid GetAdminId() =>
            Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        [HttpPut("{claimId}/review")]
        public async Task<IActionResult> SetUnderReview(Guid claimId, [FromBody] ClaimReviewDTO dto)
        {
            var adminId = GetAdminId();
            await _auditService.LogAsync("ClaimSetToReview", "Claim", claimId, adminId, dto.Notes);

            _logger.LogInformation("Admin {AdminId} set claim {ClaimId} to Under Review", adminId, claimId);

            return Ok(new { message = "Claim set to Under Review" });
        }

        [HttpPut("{claimId}/approve")]
        public async Task<IActionResult> ApproveClaim(Guid claimId, [FromBody] ClaimApprovalDTO dto)
        {
            var adminId = GetAdminId();

            // Publish event → Claims Service will transition status
            await _bus.Publish(new ClaimApprovedEvent(claimId, adminId, dto.Notes ?? "", DateTime.UtcNow));

            await _auditService.LogAsync("ClaimApproved", "Claim", claimId, adminId,
                $"Approved. Amount: {dto.ApprovedAmount}. Notes: {dto.Notes}");

            _logger.LogInformation("Admin {AdminId} approved claim {ClaimId}", adminId, claimId);

            return Ok(new { message = "Claim approved successfully" });
        }

        [HttpPut("{claimId}/reject")]
        public async Task<IActionResult> RejectClaim(Guid claimId, [FromBody] ClaimRejectionDTO dto)
        {
            var adminId = GetAdminId();

            // Publish event → Claims Service will transition status
            await _bus.Publish(new ClaimRejectedEvent(claimId, adminId, dto.Reason, DateTime.UtcNow));

            await _auditService.LogAsync("ClaimRejected", "Claim", claimId, adminId,
                $"Rejected. Reason: {dto.Reason}");

            _logger.LogInformation("Admin {AdminId} rejected claim {ClaimId}. Reason: {Reason}", adminId, claimId, dto.Reason);

            return Ok(new { message = "Claim rejected successfully" });
        }
    }
}
