using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartSure.AdminService.DTOs;
using SmartSure.AdminService.Services;
using SmartSure.Shared.Contracts.Events;
using System.Security.Claims;
using SmartSure.Shared.Contracts.Exceptions;

namespace SmartSure.AdminService.Controllers
{
    /// <summary>
    /// Represent or implements AdminClaimsController.
    /// </summary>
    [ApiController]
    [Route("admin/claims")]
    [Authorize(Roles = "Admin")]
    public class AdminClaimsController : ControllerBase
    {
        private readonly IAuditService _auditService;
        private readonly IBus _bus;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AdminClaimsController> _logger;

        public AdminClaimsController(IAuditService auditService, IBus bus, IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<AdminClaimsController> logger)
        {
            _auditService = auditService;
            _bus = bus;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _logger = logger;
        }
        
        private string GetAccessToken() => Request.Headers["Authorization"].ToString();

        /// <summary>
        /// Performs the GetAllClaims operation.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAllClaims()
        {
            _logger.LogInformation("Admin requesting all claims from Claims Service");
            
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Add("Authorization", GetAccessToken());
            var gatewayUrl = _configuration["Gateway:Url"] ?? "http://localhost:5057";

            try
            {
                var response = await client.GetAsync($"{gatewayUrl}/claims/all");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return Content(content, "application/json");
                }
                throw new HttpServiceException(await response.Content.ReadAsStringAsync(), (int)response.StatusCode);
            }
            catch (SmartSureException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling Claims Service");
                throw new BusinessRuleException("Error communicating with Claims Service");
            }
        }

        /// <summary>
        /// Performs the GetClaimStatistics operation.
        /// </summary>
        [HttpGet("statistics")]
        public async Task<IActionResult> GetClaimStatistics()
        {
            _logger.LogInformation("Admin requesting claim statistics from Claims Service");

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Add("Authorization", GetAccessToken());
            var gatewayUrl = _configuration["Gateway:Url"] ?? "http://localhost:5057";

            try
            {
                var response = await client.GetAsync($"{gatewayUrl}/claims/all");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    // Parse and aggregate stats
                    var claims = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement[]>(content);
                    if (claims == null) return Ok(new { total = 0, pending = 0, approved = 0, rejected = 0, underReview = 0 });

                    var stats = new
                    {
                        total = claims.Length,
                        pending = claims.Count(c => c.TryGetProperty("status", out var s) && s.GetString() == "Pending"),
                        underReview = claims.Count(c => c.TryGetProperty("status", out var s) && s.GetString() == "UnderReview"),
                        approved = claims.Count(c => c.TryGetProperty("status", out var s) && s.GetString() == "Approved"),
                        rejected = claims.Count(c => c.TryGetProperty("status", out var s) && s.GetString() == "Rejected")
                    };
                    return Ok(stats);
                }
                return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating claim statistics");
                return StatusCode(500, new { message = "Error calculating claim statistics" });
            }
        }

        private Guid GetAdminId() =>
            Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        /// <summary>
        /// Performs the SetUnderReview operation.
        /// </summary>
        [HttpPut("{claimId}/review")]
        public async Task<IActionResult> SetUnderReview(Guid claimId, [FromBody] ClaimReviewDTO dto)
        {
            var adminId = GetAdminId();
            await _auditService.LogAsync("ClaimSetToReview", "Claim", claimId, adminId, dto.Notes);

            _logger.LogInformation("Admin {AdminId} set claim {ClaimId} to Under Review", adminId, claimId);

            return Ok(new { message = "Claim set to Under Review" });
        }

        /// <summary>
        /// Performs the ApproveClaim operation.
        /// </summary>
        [HttpPut("{claimId}/approve")]
        public async Task<IActionResult> ApproveClaim(Guid claimId, [FromBody] ClaimApprovalDTO dto)
        {
            var adminId = GetAdminId();
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Add("Authorization", GetAccessToken());
            var gatewayUrl = _configuration["Gateway:Url"] ?? "http://localhost:5057";

            try
            {
                var response = await client.PutAsJsonAsync($"{gatewayUrl}/claims/{claimId}/approve", dto);
                if (response.IsSuccessStatusCode)
                {
                    await _auditService.LogAsync("ClaimApproved", "Claim", claimId, adminId,
                        $"Approved. Amount: {dto.ApprovedAmount}. Notes: {dto.Notes}");
                    
                    _logger.LogInformation("Admin {AdminId} approved claim {ClaimId} via HTTP", adminId, claimId);
                    return Ok(new { message = "Claim approved successfully" });
                }
                throw new HttpServiceException(await response.Content.ReadAsStringAsync(), (int)response.StatusCode);
            }
            catch (SmartSureException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling Claims Service for approval");
                throw new BusinessRuleException("Error communicating with Claims Service");
            }
        }

        /// <summary>
        /// Performs the RejectClaim operation.
        /// </summary>
        [HttpPut("{claimId}/reject")]
        public async Task<IActionResult> RejectClaim(Guid claimId, [FromBody] ClaimRejectionDTO dto)
        {
            var adminId = GetAdminId();
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Add("Authorization", GetAccessToken());
            var gatewayUrl = _configuration["Gateway:Url"] ?? "http://localhost:5057";

            try
            {
                var response = await client.PutAsJsonAsync($"{gatewayUrl}/claims/{claimId}/reject", dto);
                if (response.IsSuccessStatusCode)
                {
                    await _auditService.LogAsync("ClaimRejected", "Claim", claimId, adminId,
                        $"Rejected. Reason: {dto.Reason}");
                    
                    _logger.LogInformation("Admin {AdminId} rejected claim {ClaimId} via HTTP", adminId, claimId);
                    return Ok(new { message = "Claim rejected successfully" });
                }
                throw new HttpServiceException(await response.Content.ReadAsStringAsync(), (int)response.StatusCode);
            }
            catch (SmartSureException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling Claims Service for rejection");
                throw new BusinessRuleException("Error communicating with Claims Service");
            }
        }
    }
}
