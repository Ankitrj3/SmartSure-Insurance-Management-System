using Microsoft.AspNetCore.Mvc;
using Moq;
using SmartSure.PolicyService.Controllers;
using SmartSure.PolicyService.DTOs;
using SmartSure.PolicyService.Services;
using Xunit;

namespace SmartSure.PolicyService.Tests
{
    public class InsuranceControllerTests
    {
        private readonly Mock<IInsuranceService> _mockService;
        private readonly InsuranceController _controller;

        public InsuranceControllerTests()
        {
            _mockService = new Mock<IInsuranceService>();
            _controller = new InsuranceController(_mockService.Object);
        }

        [Fact]
        public async Task GetTypes_ReturnsOk_WithTypeList()
        {
            var types = new List<InsuranceTypeDTO> { new InsuranceTypeDTO { Name = "Home" } };
            _mockService.Setup(s => s.GetAllTypesAsync()).ReturnsAsync(types);

            var result = await _controller.GetTypes();

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(types, okResult.Value);
        }

        [Fact]
        public async Task GetType_ReturnsOk_WhenFound()
        {
            var typeId = Guid.NewGuid();
            var type = new InsuranceTypeDTO { TypeId = typeId, Name = "Home" };
            _mockService.Setup(s => s.GetTypeByIdAsync(typeId)).ReturnsAsync(type);

            var result = await _controller.GetType(typeId);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(type, okResult.Value);
        }

        [Fact]
        public async Task CreateType_ReturnsCreated()
        {
            var dto = new CreateInsuranceTypeDTO { Name = "Home" };
            var response = new InsuranceTypeDTO { TypeId = Guid.NewGuid(), Name = "Home" };
            _mockService.Setup(s => s.CreateTypeAsync(dto)).ReturnsAsync(response);

            var result = await _controller.CreateType(dto);

            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(response, createdResult.Value);
        }

        [Fact]
        public async Task UpdateType_ReturnsOk()
        {
            var typeId = Guid.NewGuid();
            var dto = new UpdateInsuranceTypeDTO { Name = "Updated Home" };

            var result = await _controller.UpdateType(typeId, dto);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Contains("successfully", okResult.Value?.ToString() ?? "");
        }

        [Fact]
        public async Task GetSubtypes_ReturnsOk()
        {
            var typeId = Guid.NewGuid();
            var subtypes = new List<InsuranceSubtypeDTO> { new InsuranceSubtypeDTO { Name = "Fire" } };
            _mockService.Setup(s => s.GetSubtypesByTypeIdAsync(typeId)).ReturnsAsync(subtypes);

            var result = await _controller.GetSubtypes(typeId);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(subtypes, okResult.Value);
        }

        [Fact]
        public async Task CreateSubtype_ReturnsOk()
        {
            var dto = new CreateInsuranceSubtypeDTO { Name = "Fire" };
            var response = new InsuranceSubtypeDTO { SubtypeId = Guid.NewGuid(), Name = "Fire" };
            _mockService.Setup(s => s.CreateSubtypeAsync(dto)).ReturnsAsync(response);

            var result = await _controller.CreateSubtype(dto);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(response, okResult.Value);
        }

        [Fact]
        public async Task UpdateSubtype_ReturnsOk()
        {
            var subtypeId = Guid.NewGuid();
            var dto = new UpdateInsuranceSubtypeDTO { Name = "Updated Fire" };

            var result = await _controller.UpdateSubtype(subtypeId, dto);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Contains("successfully", okResult.Value?.ToString() ?? "");
        }
    }
}
