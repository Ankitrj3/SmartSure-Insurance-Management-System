using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartSure.PolicyService.DTOs;
using SmartSure.PolicyService.Services;
using System.Security.Claims;
using SmartSure.Shared.Contracts.Exceptions;

namespace SmartSure.PolicyService.Controllers
{
    /// <summary>
    /// Represent or implements PoliciesController.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PoliciesController : ControllerBase
    {
        private readonly IPolicyMgmtService _service;
        private readonly ILogger<PoliciesController> _logger;

        public PoliciesController(IPolicyMgmtService service, ILogger<PoliciesController> logger)
        {
            _service = service;
            _logger = logger;
        }

        private Guid GetUserId()
        {
            var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdStr, out var userId))
                throw new UnauthorizedAccessException("User context is missing or invalid.");
            return userId;
        }

        /// <summary>
        /// Performs the GetMyPolicies operation.
        /// </summary>
        [HttpGet("/policies")]
        public async Task<IActionResult> GetMyPolicies([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var userId = GetUserId();
            var policies = await _service.GetUserPoliciesAsync(userId, page, pageSize);
            return Ok(policies);
        }

        /// <summary>
        /// Performs the GetAllPolicies operation.
        /// </summary>
        [HttpGet("/policies/all")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllPolicies([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var policies = await _service.GetAllPoliciesAsync(page, pageSize);
            return Ok(policies);
        }

        /// <summary>
        /// Performs the GetPolicy operation.
        /// </summary>
        [HttpGet("/policies/{policyId}")]
        public async Task<IActionResult> GetPolicy(Guid policyId)
        {
            var policy = await _service.GetPolicyByIdAsync(policyId);
            if (policy == null) return NotFound();
            return Ok(policy);
        }

        /// <summary>
        /// Calculate a quote (IDV + premium) without creating a policy.
        /// The user's frontend calls this first to display the calculated values.
        /// </summary>
        [HttpPost("/policies/quote")]
        public async Task<IActionResult> GetQuote([FromBody] CreatePolicyDTO dto)
        {
            try
            {
                var quote = await _service.CalculateQuoteAsync(dto);
                return Ok(quote);
            }
            catch (SmartSureException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating quote");
                throw new BusinessRuleException(ex.Message);
            }
        }

        /// <summary>
        /// Performs the BuyPolicy operation.
        /// </summary>
        [HttpPost("/policies")]
        public async Task<IActionResult> BuyPolicy(CreatePolicyDTO dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = string.Join(" | ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));
                _logger.LogWarning("Invalid BuyPolicy request: {Errors}. Payload: {@DTO}", errors, dto);
                return BadRequest(ModelState);
            }

            try 
            {
                var userIdString = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdString)) return Unauthorized("User identifier not found in token");
                
                var userId = Guid.Parse(userIdString);
                _logger.LogInformation("Creating policy for user {UserId} with Subtype {SubtypeId}", userId, dto.SubtypeId);

                var policy = await _service.CreatePolicyAsync(userId, dto);
                return Ok(policy);
            }
            catch (SmartSureException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating policy for user");
                throw new BusinessRuleException(ex.Message);
            }
        }

        /// <summary>
        /// Activate a pending policy after successful payment.
        /// </summary>
        [HttpPut("/policies/{policyId}/activate")]
        public async Task<IActionResult> ActivatePolicy(Guid policyId)
        {
            try
            {
                await _service.ActivatePolicyAsync(policyId);
                return Ok(new { message = "Policy activated successfully" });
            }
            catch (SmartSureException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new BusinessRuleException(ex.Message);
            }
        }

        /// <summary>
        /// Performs the CancelPolicy operation.
        /// </summary>
        [HttpPut("/policies/{policyId}/cancel")]
        public async Task<IActionResult> CancelPolicy(Guid policyId)
        {
            await _service.CancelPolicyAsync(policyId);
            return Ok(new { message = "Policy cancelled successfully" });
        }

        /// <summary>
        /// Performs the FailPolicy operation.
        /// </summary>
        [HttpPut("/policies/{policyId}/fail")]
        public async Task<IActionResult> FailPolicy(Guid policyId)
        {
            await _service.FailPolicyAsync(policyId);
            return Ok(new { message = "Policy marked as failed" });
        }

        /// <summary>
        /// Performs the DeletePolicy operation.
        /// </summary>
        [HttpDelete("/policies/{policyId}")]
        public async Task<IActionResult> DeletePolicy(Guid policyId)
        {
            await _service.DeletePolicyAsync(policyId);
            return Ok(new { message = "Policy deleted successfully" });
        }

        /// <summary>
        /// Performs the GetDetails operation.
        /// </summary>
        [HttpGet("/policies/{policyId}/details")]
        public async Task<IActionResult> GetDetails(Guid policyId)
        {
            var detail = await _service.GetPolicyDetailsAsync(policyId);
            if (detail == null) return NotFound();
            return Ok(detail);
        }

        /// <summary>
        /// Performs the SaveDetails operation.
        /// </summary>
        [HttpPost("/policies/{policyId}/details")]
        public async Task<IActionResult> SaveDetails(Guid policyId, SavePolicyDetailDTO dto)
        {
            await _service.SavePolicyDetailsAsync(policyId, dto);
            return Ok(new { message = "Policy details saved successfully" });
        }

        /// <summary>
        /// Performs the UpdateDetails operation.
        /// </summary>
        [HttpPut("/policies/{policyId}/details")]
        public async Task<IActionResult> UpdateDetails(Guid policyId, SavePolicyDetailDTO dto)
        {
            await _service.SavePolicyDetailsAsync(policyId, dto);
            return Ok(new { message = "Policy details updated successfully" });
        }

        /// <summary>
        /// Performs the GetPremium operation.
        /// </summary>
        [HttpGet("/policies/{policyId}/premium")]
        public async Task<IActionResult> GetPremium(Guid policyId)
        {
            var premium = await _service.GetPremiumAmountAsync(policyId);
            return Ok(new { premiumAmount = premium });
        }

        /// <summary>
        /// Performs the GetHomeDetail operation.
        /// </summary>
        [HttpGet("/home-details/{policyId}")]
        public async Task<IActionResult> GetHomeDetail(Guid policyId)
        {
            var detail = await _service.GetHomeDetailAsync(policyId);
            if (detail == null) return NotFound();
            return Ok(detail);
        }

        /// <summary>
        /// Performs the GetVehicleDetail operation.
        /// </summary>
        [HttpGet("/vehicle-details/{policyId}")]
        public async Task<IActionResult> GetVehicleDetail(Guid policyId)
        {
            var detail = await _service.GetVehicleDetailAsync(policyId);
            if (detail == null) return NotFound();
            return Ok(detail);
        }

        /// <summary>
        /// Terminate a policy (used for theft/total loss claims)
        /// </summary>
        [HttpPatch("/policies/{policyId}/terminate")]
        public async Task<IActionResult> TerminatePolicy(Guid policyId)
        {
            try
            {
                await _service.TerminatePolicyAsync(policyId);
                return Ok(new { message = "Policy terminated successfully" });
            }
            catch (SmartSureException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new BusinessRuleException(ex.Message);
            }
        }

        /// <summary>
        /// Increment approved claims count for a policy
        /// </summary>
        [HttpPatch("/policies/{policyId}/increment-claim-count")]
        public async Task<IActionResult> IncrementClaimCount(Guid policyId)
        {
            try
            {
                await _service.IncrementApprovedClaimsCountAsync(policyId);
                return Ok(new { message = "Claim count incremented successfully" });
            }
            catch (SmartSureException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new BusinessRuleException(ex.Message);
            }
        }
    }
}
