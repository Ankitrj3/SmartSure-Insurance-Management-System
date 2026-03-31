using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartSure.Shared.Contracts.Exceptions;

namespace SmartSure.AdminService.Controllers
{
    /// <summary>
    /// Represent or implements AdminPoliciesController.
    /// </summary>
    [ApiController]
    [Route("admin/policies")]
    [Authorize(Roles = "Admin")]
    public class AdminPoliciesController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AdminPoliciesController> _logger;

        public AdminPoliciesController(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<AdminPoliciesController> logger)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _logger = logger;
        }
        
        private string GetAccessToken() => Request.Headers["Authorization"].ToString();

        /// <summary>
        /// Performs the GetAllPolicies operation.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAllPolicies()
        {
            _logger.LogInformation("Admin requesting all policies from Policy Service");
            
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Add("Authorization", GetAccessToken());
            var gatewayUrl = _configuration["Gateway:Url"] ?? "http://localhost:5057";

            try
            {
                var response = await client.GetAsync($"{gatewayUrl}/policies/all");
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
                _logger.LogError(ex, "Error calling Policy Service");
                throw new BusinessRuleException("Error communicating with Policy Service");
            }
        }
    }
}
