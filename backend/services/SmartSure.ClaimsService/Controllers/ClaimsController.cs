using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartSure.ClaimsService.DTOs;
using SmartSure.ClaimsService.Services;
using System.Security.Claims;
using SmartSure.Shared.Contracts.Exceptions;

namespace SmartSure.ClaimsService.Controllers
{
    [ApiController]
    [Route("claims")]
    [Authorize]
    /// <summary>
    /// Represent or implements ClaimsController.
    /// </summary>
    public class ClaimsController : ControllerBase
    {
        private readonly IClaimService _claimService;

        public ClaimsController(IClaimService claimService)
        {
            _claimService = claimService;
        }

        private Guid GetUserId() =>
            Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        [HttpGet]
        /// <summary>
        /// Performs the GetMyClaims operation.
        /// </summary>
        public async Task<IActionResult> GetMyClaims()
        {
            var userId = GetUserId();
            var claims = await _claimService.GetUserClaimsAsync(userId);
            return Ok(claims);
        }

        [HttpGet("all")]
        [Authorize(Roles = "Admin")]
        /// <summary>
        /// Performs the GetAllClaims operation.
        /// </summary>
        public async Task<IActionResult> GetAllClaims()
        {
            var claims = await _claimService.GetAllClaimsAsync();
            return Ok(claims);
        }

        [HttpGet("{claimId}")]
        /// <summary>
        /// Performs the GetClaim operation.
        /// </summary>
        public async Task<IActionResult> GetClaim(Guid claimId)
        {
            var claim = await _claimService.GetClaimByIdAsync(claimId);
            if (claim == null) return NotFound(new { message = "Claim not found" });
            return Ok(claim);
        }

        [HttpPost]
        /// <summary>
        /// Performs the CreateClaim operation.
        /// </summary>
        public async Task<IActionResult> CreateClaim([FromBody] CreateClaimDTO dto)
        {
            var userId = GetUserId();
            var claim = await _claimService.CreateClaimAsync(userId, dto);
            return CreatedAtAction(nameof(GetClaim), new { claimId = claim.ClaimId }, claim);
        }

        [HttpPut("{claimId}")]
        /// <summary>
        /// Performs the UpdateClaim operation.
        /// </summary>
        public async Task<IActionResult> UpdateClaim(Guid claimId, [FromBody] UpdateClaimDTO dto)
        {
            var claim = await _claimService.UpdateClaimAsync(claimId, dto);
            return Ok(claim);
        }

        [HttpPut("{claimId}/submit")]
        /// <summary>
        /// Performs the SubmitClaim operation.
        /// </summary>
        public async Task<IActionResult> SubmitClaim(Guid claimId)
        {
            var userId = GetUserId();
            await _claimService.SubmitClaimAsync(claimId, userId);
            return Ok(new { message = "Claim submitted for review" });
        }

        [HttpGet("{claimId}/history")]
        /// <summary>
        /// Performs the GetClaimHistory operation.
        /// </summary>
        public async Task<IActionResult> GetClaimHistory(Guid claimId)
        {
            var history = await _claimService.GetClaimHistoryAsync(claimId);
            return Ok(history);
        }

        [HttpPut("{claimId}/approve")]
        [Authorize(Roles = "Admin")]
        /// <summary>
        /// Performs the ApproveClaim operation.
        /// </summary>
        public async Task<IActionResult> ApproveClaim(Guid claimId, [FromBody] ApproveClaimDTO dto)
        {
            var adminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "Admin";
            await _claimService.ApproveClaimAsync(claimId, dto.ApprovedAmount, dto.Notes, adminId);
            return Ok(new { message = "Claim approved successfully" });
        }

        [HttpPut("{claimId}/reject")]
        [Authorize(Roles = "Admin")]
        /// <summary>
        /// Performs the RejectClaim operation.
        /// </summary>
        public async Task<IActionResult> RejectClaim(Guid claimId, [FromBody] RejectClaimDTO dto)
        {
            var adminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "Admin";
            await _claimService.RejectClaimAsync(claimId, dto.Reason, adminId);
            return Ok(new { message = "Claim rejected successfully" });
        }

        [HttpGet("by-policy/{policyId}")]
        /// <summary>
        /// Performs the GetClaimsByPolicy operation.
        /// </summary>
        public async Task<IActionResult> GetClaimsByPolicy(Guid policyId)
        {
            var claims = await _claimService.GetClaimsByPolicyAsync(policyId);
            return Ok(claims);
        }

        [HttpPut("{claimId}/status")]
        [Authorize(Roles = "Admin")]
        /// <summary>
        /// Performs the UpdateClaimStatus operation.
        /// </summary>
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
