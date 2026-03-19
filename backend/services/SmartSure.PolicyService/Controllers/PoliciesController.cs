using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartSure.PolicyService.DTOs;
using SmartSure.PolicyService.Services;
using System.Security.Claims;

namespace SmartSure.PolicyService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PoliciesController : ControllerBase
    {
        private readonly IPolicyMgmtService _service;

        public PoliciesController(IPolicyMgmtService service)
        {
            _service = service;
        }

        private Guid GetUserId()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.Parse(userId);
        }

        [HttpGet]
        public async Task<IActionResult> GetMyPolicies()
        {
            var userId = GetUserId();
            var policies = await _service.GetUserPoliciesAsync(userId);
            return Ok(policies);
        }

        [HttpGet("{policyId}")]
        public async Task<IActionResult> GetPolicy(Guid policyId)
        {
            var policy = await _service.GetPolicyByIdAsync(policyId);
            if (policy == null) return NotFound();
            return Ok(policy);
        }

        [HttpPost]
        public async Task<IActionResult> BuyPolicy(CreatePolicyDTO dto)
        {
            var userId = GetUserId();
            var policy = await _service.CreatePolicyAsync(userId, dto);
            return CreatedAtAction(nameof(GetPolicy), new { policyId = policy.PolicyId }, policy);
        }

        [HttpPut("{policyId}/cancel")]
        public async Task<IActionResult> CancelPolicy(Guid policyId)
        {
            await _service.CancelPolicyAsync(policyId);
            return Ok(new { message = "Policy cancelled successfully" });
        }

        [HttpGet("{policyId}/details")]
        public async Task<IActionResult> GetDetails(Guid policyId)
        {
            var detail = await _service.GetPolicyDetailsAsync(policyId);
            if (detail == null) return NotFound();
            return Ok(detail);
        }

        [HttpPost("{policyId}/details")]
        [HttpPut("{policyId}/details")]
        public async Task<IActionResult> SaveDetails(Guid policyId, SavePolicyDetailDTO dto)
        {
            await _service.SavePolicyDetailsAsync(policyId, dto);
            return Ok(new { message = "Policy details saved successfully" });
        }

        [HttpGet("{policyId}/premium")]
        public async Task<IActionResult> GetPremium(Guid policyId)
        {
            var premium = await _service.GetPremiumAmountAsync(policyId);
            return Ok(new { premiumAmount = premium });
        }

        [HttpGet("home-details/{policyId}")]
        public async Task<IActionResult> GetHomeDetail(Guid policyId)
        {
            var detail = await _service.GetHomeDetailAsync(policyId);
            if (detail == null) return NotFound();
            return Ok(detail);
        }

        [HttpPost("home-details/{policyId}")]
        public async Task<IActionResult> SaveHomeDetail(Guid policyId, CreateHomeDetailDTO dto)
        {
            await _service.SaveHomeDetailAsync(policyId, dto);
            return Ok(new { message = "Home detail saved successfully" });
        }

        [HttpGet("vehicle-details/{policyId}")]
        public async Task<IActionResult> GetVehicleDetail(Guid policyId)
        {
            var detail = await _service.GetVehicleDetailAsync(policyId);
            if (detail == null) return NotFound();
            return Ok(detail);
        }

        [HttpPost("vehicle-details/{policyId}")]
        public async Task<IActionResult> SaveVehicleDetail(Guid policyId, CreateVehicleDetailDTO dto)
        {
            await _service.SaveVehicleDetailAsync(policyId, dto);
            return Ok(new { message = "Vehicle detail saved successfully" });
        }
    }
}
