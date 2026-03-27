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
        public async Task<IActionResult> GetMyClaims()
        {
            var userId = GetUserId();
            var claims = await _claimService.GetUserClaimsAsync(userId);
            return Ok(claims);
        }

        [HttpGet("all")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllClaims()
        {
            var claims = await _claimService.GetAllClaimsAsync();
            return Ok(claims);
        }

        [HttpGet("{claimId}")]
        public async Task<IActionResult> GetClaim(Guid claimId)
        {
            var claim = await _claimService.GetClaimByIdAsync(claimId);
            if (claim == null) return NotFound(new { message = "Claim not found" });
            return Ok(claim);
        }

        [HttpPost]
        public async Task<IActionResult> CreateClaim([FromBody] CreateClaimDTO dto)
        {
            var userId = GetUserId();
            var claim = await _claimService.CreateClaimAsync(userId, dto);
            return CreatedAtAction(nameof(GetClaim), new { claimId = claim.ClaimId }, claim);
        }

        [HttpPut("{claimId}")]
        public async Task<IActionResult> UpdateClaim(Guid claimId, [FromBody] UpdateClaimDTO dto)
        {
            var claim = await _claimService.UpdateClaimAsync(claimId, dto);
            return Ok(claim);
        }

        [HttpPut("{claimId}/submit")]
        public async Task<IActionResult> SubmitClaim(Guid claimId)
        {
            var userId = GetUserId();
            await _claimService.SubmitClaimAsync(claimId, userId);
            return Ok(new { message = "Claim submitted for review" });
        }

        [HttpGet("{claimId}/history")]
        public async Task<IActionResult> GetClaimHistory(Guid claimId)
        {
            var history = await _claimService.GetClaimHistoryAsync(claimId);
            return Ok(history);
        }

        [HttpPut("{claimId}/approve")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ApproveClaim(Guid claimId, [FromBody] ApproveClaimDTO dto)
        {
            var adminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "Admin";
            await _claimService.ApproveClaimAsync(claimId, dto.ApprovedAmount, dto.Notes, adminId);
            return Ok(new { message = "Claim approved successfully" });
        }

        [HttpPut("{claimId}/reject")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RejectClaim(Guid claimId, [FromBody] RejectClaimDTO dto)
        {
            var adminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "Admin";
            await _claimService.RejectClaimAsync(claimId, dto.Reason, adminId);
            return Ok(new { message = "Claim rejected successfully" });
        }

        [HttpGet("by-policy/{policyId}")]
        public async Task<IActionResult> GetClaimsByPolicy(Guid policyId)
        {
            var claims = await _claimService.GetClaimsByPolicyAsync(policyId);
            return Ok(claims);
        }
    }
}
