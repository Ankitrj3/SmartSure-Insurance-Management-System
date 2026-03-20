using IdentityService.Controllers;
using IdentityService.DTOs;
using IdentityService.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;
using Xunit;

namespace SmartSure.IdentityService.Tests
{
    public class AuthControllerTests
    {
        private readonly Mock<IAuthService> _mockAuthService;
        private readonly Mock<IGoogleAuthService> _mockGoogleAuthService;
        private readonly AuthController _controller;

        public AuthControllerTests()
        {
            _mockAuthService = new Mock<IAuthService>();
            _mockGoogleAuthService = new Mock<IGoogleAuthService>();
            _controller = new AuthController(_mockAuthService.Object, _mockGoogleAuthService.Object);
        }

        [Fact]
        public async Task Register_ReturnsOk_OnSuccess()
        {
            var dto = new RegisterDTO { Email = "test@test.com" };
            _mockAuthService.Setup(x => x.Register(dto)).ReturnsAsync("Registration successful");

            var result = await _controller.Register(dto);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Contains("Registration successful", okResult.Value?.ToString() ?? "");
        }

        [Fact]
        public async Task Login_ReturnsOk_WithTokens()
        {
            var dto = new LoginDTO { Email = "test@test.com", Password = "Password123!" };
            var response = new TokenResponseDTO { Token = "access", RefreshToken = "refresh" };
            _mockAuthService.Setup(x => x.Login(dto)).ReturnsAsync(response);

            var result = await _controller.Login(dto);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(response, okResult.Value);
        }

        [Fact]
        public async Task GetProfile_ReturnsUser_WhenAuthorized()
        {
            var userId = Guid.NewGuid().ToString();
            var userDto = new UserDTO { UserId = Guid.Parse(userId), Email = "test@test.com" };
            _mockAuthService.Setup(x => x.GetProfile(userId)).ReturnsAsync(userDto);

            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] {
                new Claim(ClaimTypes.NameIdentifier, userId)
            }));
            _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } };

            var result = await _controller.GetProfile();

              var okResult = Assert.IsType<OkObjectResult>(result);
             Assert.Equal(userDto, okResult.Value);
         }
 
         [Fact]
         public async Task UpdateProfile_ReturnsOk_OnSuccess()
         {
              var userId = Guid.NewGuid().ToString();
             var dto = new UpdateUserDTO { FullName = "New Name" };
             _mockAuthService.Setup(x => x.UpdateProfile(userId, dto)).Returns(Task.CompletedTask);
 
             var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] {
                 new Claim(ClaimTypes.NameIdentifier, userId)
             }));
             _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } };
 
              var result = await _controller.UpdateProfile(dto);
 
              var okResult = Assert.IsType<OkObjectResult>(result);
             Assert.Contains("Profile updated successfully", okResult.Value?.ToString() ?? "");
         }
 
         [Fact]
         public async Task ChangePassword_ReturnsOk_OnSuccess()
         {
              var userId = Guid.NewGuid().ToString();
             var dto = new ChangePasswordDTO { OldPassword = "old", NewPassword = "new" };
             _mockAuthService.Setup(x => x.ChangePassword(userId, dto)).Returns(Task.CompletedTask);
 
             var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] {
                 new Claim(ClaimTypes.NameIdentifier, userId)
             }));
             _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } };
 
              var result = await _controller.ChangePassword(dto);
 
              var okResult = Assert.IsType<OkObjectResult>(result);
             Assert.Contains("Password changed successfully", okResult.Value?.ToString() ?? "");
         }
 
         [Fact]
         public void Logout_ReturnsOk()
         {
              var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] {
                 new Claim(ClaimTypes.NameIdentifier, "someuser")
             }));
             _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } };
 
              var result = _controller.Logout();
 
              var okResult = Assert.IsType<OkObjectResult>(result);
             Assert.Contains("Logged out successfully", okResult.Value?.ToString() ?? "");
         }
 
         [Fact]
         public async Task Refresh_ReturnsNewTokens_OnValidRequest()
         {
              var dto = new RefreshRequestDTO { RefreshToken = "old_refresh" };
             var response = new TokenResponseDTO { Token = "new_access", RefreshToken = "new_refresh" };
             _mockAuthService.Setup(x => x.Refresh(dto.RefreshToken)).ReturnsAsync(response);
 
              var result = await _controller.Refresh(dto);
 
              var okResult = Assert.IsType<OkObjectResult>(result);
             Assert.Equal(response, okResult.Value);
         }
 
         [Fact]
         public async Task Register_ReturnsBadRequest_OnServiceError()
         {
              var dto = new RegisterDTO { Email = "fail@test.com" };
             _mockAuthService.Setup(x => x.Register(dto)).ThrowsAsync(new System.Exception("Duplicate email"));
 
              var result = await _controller.Register(dto);
 
              var badRequest = Assert.IsType<BadRequestObjectResult>(result);
             Assert.Contains("Duplicate email", badRequest.Value?.ToString() ?? "");
         }
     }
 }
