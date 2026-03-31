using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartSure.Shared.Contracts.Exceptions;

namespace SmartSure.AdminService.Controllers
{
    /// <summary>
    /// Represent or implements AdminUsersController.
    /// </summary>
    [ApiController]
    [Route("admin/users")]
    [Authorize(Roles = "Admin")]
    public class AdminUsersController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AdminUsersController> _logger;

        public AdminUsersController(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<AdminUsersController> logger)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _logger = logger;
        }

        private string GetAccessToken() => Request.Headers["Authorization"].ToString();

        /// <summary>
        /// Performs the GetUsers operation.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            _logger.LogInformation("Admin requesting user list from Identity Service");
            
            var client = _httpClientFactory.CreateClient("IdentityClient");
            client.DefaultRequestHeaders.Add("Authorization", GetAccessToken());

            var gatewayUrl = _configuration["Gateway:Url"] ?? "http://localhost:5057";
            
            try
            {
                var response = await client.GetAsync($"{gatewayUrl}/auth/users");
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
                _logger.LogError(ex, "Error calling Identity Service");
                throw new BusinessRuleException("Error communicating with Identity Service");
            }
        }

        /// <summary>
        /// Performs the GetUser operation.
        /// </summary>
        [HttpGet("{userId}")]
        public async Task<IActionResult> GetUser(Guid userId)
        {
            _logger.LogInformation("Admin requesting user detail for {UserId}", userId);
            
            var client = _httpClientFactory.CreateClient("IdentityClient");
            client.DefaultRequestHeaders.Add("Authorization", GetAccessToken());
            var gatewayUrl = _configuration["Gateway:Url"] ?? "http://localhost:5057";

            try
            {
                var response = await client.GetAsync($"{gatewayUrl}/auth/users/{userId}"); // Assuming this endpoint exists or should map
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
                _logger.LogError(ex, "Error calling Identity Service");
                throw new BusinessRuleException("Error communicating with Identity Service");
            }
        }

        /// <summary>
        /// Performs the DeactivateUser operation.
        /// </summary>
        [HttpDelete("{userId}")]
        public async Task<IActionResult> DeactivateUser(Guid userId)
        {
            _logger.LogInformation("Admin deleting user {UserId}", userId);
            
            var client = _httpClientFactory.CreateClient("IdentityClient");
            client.DefaultRequestHeaders.Add("Authorization", GetAccessToken());
            var gatewayUrl = _configuration["Gateway:Url"] ?? "http://localhost:5057";

            try
            {
                var response = await client.DeleteAsync($"{gatewayUrl}/auth/users/{userId}");
                if (response.IsSuccessStatusCode)
                {
                    return Ok(new { message = "User deleted successfully" });
                }
                throw new HttpServiceException(await response.Content.ReadAsStringAsync(), (int)response.StatusCode);
            }
            catch (SmartSureException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling Identity Service");
                throw new BusinessRuleException("Error communicating with Identity Service");
            }
        }
    }
}
