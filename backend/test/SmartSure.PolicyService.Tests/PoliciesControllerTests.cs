using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SmartSure.PolicyService.Controllers;
using SmartSure.PolicyService.DTOs;
using SmartSure.PolicyService.Services;
using System.Security.Claims;
using Xunit;

namespace SmartSure.PolicyService.Tests
{
    public class PoliciesControllerTests
    {
        private readonly Mock<IPolicyMgmtService> _mockService;
        private readonly PoliciesController _controller;

        public PoliciesControllerTests()
        {
            _mockService = new Mock<IPolicyMgmtService>();
            _controller = new PoliciesController(_mockService.Object);
        }

        private void MockUser(Guid userId)
        {
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString())
            }));
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
        }

        [Fact]
        public async Task GetMyPolicies_ReturnsOk()
        {
            var userId = Guid.NewGuid();
            MockUser(userId);
            var policies = new List<PolicyDTO> { new PolicyDTO { PolicyId = Guid.NewGuid() } };
            _mockService.Setup(s => s.GetUserPoliciesAsync(userId)).ReturnsAsync(policies);

            var result = await _controller.GetMyPolicies();

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(policies, okResult.Value);
        }

        [Fact]
        public async Task BuyPolicy_ReturnsCreatedAt()
        {
            var userId = Guid.NewGuid();
            MockUser(userId);
            var dto = new CreatePolicyDTO { SubtypeId = Guid.NewGuid(), Duration = 12 };
            var response = new PolicyDTO { PolicyId = Guid.NewGuid(), UserId = userId, Status = "Active" };
            _mockService.Setup(s => s.CreatePolicyAsync(userId, dto)).ReturnsAsync(response);

            var result = await _controller.BuyPolicy(dto);

            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(response, createdResult.Value);
        }

        [Fact]
        public async Task CancelPolicy_ReturnsOk()
        {
            var policyId = Guid.NewGuid();

            var result = await _controller.CancelPolicy(policyId);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Contains("successfully", okResult.Value?.ToString() ?? "");
        }

        [Fact]
        public async Task SaveDetails_ReturnsOk()
        {
            var policyId = Guid.NewGuid();
            var dto = new SavePolicyDetailDTO { TermsAndConditions = "Updated terms" };

            var result = await _controller.SaveDetails(policyId, dto);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Contains("successfully", okResult.Value?.ToString() ?? "");
        }

        [Fact]
        public async Task GetPremium_ReturnsAmount()
        {
            var policyId = Guid.NewGuid();
            _mockService.Setup(s => s.GetPremiumAmountAsync(policyId)).ReturnsAsync(100.50m);

            var result = await _controller.GetPremium(policyId);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Contains("100.50", okResult.Value?.ToString() ?? "");
        }

        [Fact]
        public async Task SaveHomeDetail_ReturnsOk()
        {
            var policyId = Guid.NewGuid();
            var dto = new CreateHomeDetailDTO { Address = "Updated home addr" };

            var result = await _controller.SaveHomeDetail(policyId, dto);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Contains("successfully", okResult.Value?.ToString() ?? "");
        }

        [Fact]
        public async Task SaveVehicleDetail_ReturnsOk()
        {
            var policyId = Guid.NewGuid();
            var dto = new CreateVehicleDetailDTO { RegistrationNumber = "ABC-123" };

            var result = await _controller.SaveVehicleDetail(policyId, dto);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Contains("successfully", okResult.Value?.ToString() ?? "");
        }
    }
}
