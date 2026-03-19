using IdentityService.Controllers;
using IdentityService.DTOs;
using IdentityService.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace SmartSure.IdentityService.Tests
{
    public class UsersControllerTests
    {
        private readonly Mock<IUserService> _mockUserService;
        private readonly UsersController _controller;

        public UsersControllerTests()
        {
            _mockUserService = new Mock<IUserService>();
            _controller = new UsersController(_mockUserService.Object);
        }

        [Fact]
        public async Task GetUsers_ReturnsOk_WithUserList()
        {
            var users = new List<UserDTO> { new UserDTO { Email = "user1@test.com" }, new UserDTO { Email = "user2@test.com" } };
            _mockUserService.Setup(x => x.GetUsers()).ReturnsAsync(users);

            var result = await _controller.GetUsers();

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(users, okResult.Value);
        }

        [Fact]
        public async Task AssignRole_ReturnsOk_OnSuccess()
        {
            var userId = Guid.NewGuid();
            var dto = new AssignRoleDTO { RoleId = Guid.NewGuid() };
            _mockUserService.Setup(x => x.AssignRole(userId, dto.RoleId)).Returns(Task.CompletedTask);

            var result = await _controller.AssignRole(userId, dto);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Contains("Role assigned successfully", okResult.Value?.ToString() ?? "");
        }

        [Fact]
        public async Task DeleteUser_ReturnsOk_OnSuccess()
        {
            var userId = Guid.NewGuid();
            _mockUserService.Setup(x => x.DeleteUser(userId)).Returns(Task.CompletedTask);

            var result = await _controller.DeleteUser(userId);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Contains("User deleted successfully", okResult.Value?.ToString() ?? "");
        }
    }
}
