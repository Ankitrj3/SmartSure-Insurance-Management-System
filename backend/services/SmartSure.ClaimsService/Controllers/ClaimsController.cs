using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartSure.ClaimsService.DTOs;
using SmartSure.ClaimsService.Services;
using System.Security.Claims;
using SmartSure.Shared.Contracts.Exceptions;

namespace SmartSure.ClaimsService.Controllers
{
    /// <summary>
    /// Represent or implements ClaimsController.
    /// </summary>
    [ApiController]
    [Route("claims")]
    [Authorize]
    public class ClaimsController : ControllerBase
    {
        private readonly IClaimService _claimService;

        public ClaimsController(IClaimService claimService)
        {
            _claimService = claimService;
        }

        private Guid GetUserId() =>
            Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        /// <summary>
        /// Performs the GetMyClaims operation.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetMyClaims([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var userId = GetUserId();
            var claims = await _claimService.GetUserClaimsAsync(userId, page, pageSize);
            return Ok(claims);
        }

        /// <summary>
        /// Performs the GetAllClaims operation.
        /// </summary>
        [HttpGet("all")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllClaims([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var claims = await _claimService.GetAllClaimsAsync(page, pageSize);
            return Ok(claims);
        }

        /// <summary>
        /// Performs the GetClaim operation.
        /// </summary>
        [HttpGet("{claimId}")]
        public async Task<IActionResult> GetClaim(Guid claimId)
        {
            var claim = await _claimService.GetClaimByIdAsync(claimId);
            if (claim == null) return NotFound(new { message = "Claim not found" });
            return Ok(claim);
        }

        /// <summary>
        /// Performs the CreateClaim operation.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateClaim([FromBody] CreateClaimDTO dto)
        {
            var userId = GetUserId();
            var claim = await _claimService.CreateClaimAsync(userId, dto);
            return CreatedAtAction(nameof(GetClaim), new { claimId = claim.ClaimId }, claim);
        }

        /// <summary>
        /// Performs the UpdateClaim operation.
        /// </summary>
        [HttpPut("{claimId}")]
        public async Task<IActionResult> UpdateClaim(Guid claimId, [FromBody] UpdateClaimDTO dto)
        {
            var claim = await _claimService.UpdateClaimAsync(claimId, dto);
            return Ok(claim);
        }

        /// <summary>
        /// Performs the SubmitClaim operation.
        /// </summary>
        [HttpPut("{claimId}/submit")]
        public async Task<IActionResult> SubmitClaim(Guid claimId)
        {
            var userId = GetUserId();
            await _claimService.SubmitClaimAsync(claimId, userId);
            return Ok(new { message = "Claim submitted for review" });
        }

        /// <summary>
        /// Performs the GetClaimHistory operation.
        /// </summary>
        [HttpGet("{claimId}/history")]
        public async Task<IActionResult> GetClaimHistory(Guid claimId)
        {
            var history = await _claimService.GetClaimHistoryAsync(claimId);
            return Ok(history);
        }

        /// <summary>
        /// Performs the ApproveClaim operation.
        /// </summary>
        [HttpPut("{claimId}/approve")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ApproveClaim(Guid claimId, [FromBody] ApproveClaimDTO dto)
        {
            var adminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "Admin";
            await _claimService.ApproveClaimAsync(claimId, dto.ApprovedAmount, dto.Notes, adminId);
            return Ok(new { message = "Claim approved successfully" });
        }

        /// <summary>
        /// Performs the RejectClaim operation.
        /// </summary>
        [HttpPut("{claimId}/reject")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RejectClaim(Guid claimId, [FromBody] RejectClaimDTO dto)
        {
            var adminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "Admin";
            await _claimService.RejectClaimAsync(claimId, dto.Reason, adminId);
            return Ok(new { message = "Claim rejected successfully" });
        }

        /// <summary>
        /// Performs the GetClaimsByPolicy operation.
        /// </summary>
        [HttpGet("by-policy/{policyId}")]
        public async Task<IActionResult> GetClaimsByPolicy(Guid policyId)
        {
            var claims = await _claimService.GetClaimsByPolicyAsync(policyId);
            return Ok(claims);
        }

        /// <summary>
        /// Performs the UpdateClaimStatus operation.
        /// </summary>
        [HttpPut("{claimId}/status")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateClaimStatus(Guid claimId, [FromBody] UpdateClaimStatusDTO dto)
        {
            var adminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "Admin";
            await _claimService.UpdateClaimStatusAsync(claimId, dto.Status, dto.Notes, adminId);
            return Ok(new { message = $"Claim status updated to {dto.Status}" });
        }
    }

    /// <summary>
    /// Represent or implements UpdateClaimStatusDTO.
    /// </summary>
    public class UpdateClaimStatusDTO
    {
        public string Status { get; set; } = "";
        public string? Notes { get; set; }
    }
}
