using IdentityService.DTOs;
using IdentityService.Helpers;
using IdentityService.Models;
using IdentityService.Repositories;
using IdentityService.Services;
using MassTransit;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Moq;
using SmartSure.Shared.Contracts.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace SmartSure.IdentityService.Tests
{
    /// <summary>
    /// Unit tests for AuthService - covering registration pipeline, authentication,
    /// password management, and profile CRUD operations.
    /// </summary>
    public class AuthServiceTests
    {
        private readonly Mock<IUserRepository> _repoMock;
        private readonly Mock<TokenService> _tokenServiceMock;
        private readonly Mock<IConfiguration> _configMock;
        private readonly IMemoryCache _memoryCache;
        private readonly Mock<IBus> _busMock;
        private readonly Mock<IEmailService> _emailServiceMock;
        private readonly Mock<IOtpService> _otpServiceMock;
        private readonly AuthService _authService;

        public AuthServiceTests()
        {
            _repoMock         = new Mock<IUserRepository>();
            _tokenServiceMock = new Mock<TokenService>();
            _configMock       = new Mock<IConfiguration>();
            _memoryCache      = new MemoryCache(new MemoryCacheOptions());
            _busMock          = new Mock<IBus>();
            _emailServiceMock = new Mock<IEmailService>();
            _otpServiceMock   = new Mock<IOtpService>();

            // Configure JWT config keys
            _configMock.Setup(c => c["Jwt:Key"]).Returns("supersecretkey1234567890abcdef!@#$");
            _configMock.Setup(c => c["Jwt:Issuer"]).Returns("SmartSure");
            _configMock.Setup(c => c["Jwt:Aud1"]).Returns("AudienceOne");
            _configMock.Setup(c => c["Jwt:Aud2"]).Returns("");
            _configMock.Setup(c => c["Jwt:Aud3"]).Returns("");
            _configMock.Setup(c => c["Jwt:Aud4"]).Returns("");
            _configMock.Setup(c => c["Jwt:Aud5"]).Returns("");

            _authService = new AuthService(
                _repoMock.Object,
                _tokenServiceMock.Object,
                _configMock.Object,
                _memoryCache,
                _busMock.Object,
                _emailServiceMock.Object,
                _otpServiceMock.Object
            );
        }

        // ── Helper ─────────────────────────────────────────────────────────────

        private static User CreateUser(string email = "test@example.com", string password = "Password123!")
        {
            return new User
            {
                UserId      = Guid.NewGuid(),
                FullName    = "Test User",
                Email       = email,
                PhoneNumber = "9876543210",
                Address     = "123 Main St",
                Password    = new Password { PasswordHash = PasswordHasher.PasswordHash(password) },
                UserRoles   = new List<UserRole>
                {
                    new UserRole { Role = new Role { RoleName = "Customer" } }
                }
            };
        }

        // ── Registration Tests ─────────────────────────────────────────────────

        [Fact]
        public async Task Register_EmailAlreadyExists_ThrowsConflictException()
        {
            // Arrange
            var dto = new RegisterDTO { Email = "exists@test.com", FullName = "Test", Password = "Pass1!", PhoneNumber = "9876543210" };
            _repoMock.Setup(r => r.GetByEmailAsync(dto.Email)).ReturnsAsync(new User());

            // Act & Assert
            await Assert.ThrowsAsync<ConflictException>(() => _authService.Register(dto));
        }

        [Fact]
        public async Task Register_NewEmail_CachesOtpAndData_ReturnsSuccessMessage()
        {
            // Arrange
            var dto = new RegisterDTO { Email = "new@test.com", FullName = "New User", Password = "Pass1!", PhoneNumber = "9876543210" };
            _repoMock.Setup(r => r.GetByEmailAsync(dto.Email)).ReturnsAsync((User)null!);
            _emailServiceMock.Setup(e => e.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _authService.Register(dto);

            // Assert
            Assert.Contains("OTP sent", result);
            Assert.True(_memoryCache.TryGetValue($"RegistrationOtp_{dto.Email}", out string? cachedOtp));
            Assert.NotNull(cachedOtp);
            Assert.True(_memoryCache.TryGetValue($"RegistrationData_{dto.Email}", out RegisterDTO? cachedDto));
            Assert.NotNull(cachedDto);
        }

        [Fact]
        public async Task Register_OtpIs6Digits()
        {
            // Arrange
            var dto = new RegisterDTO { Email = "otp@test.com", FullName = "OTP User", Password = "Pass1!", PhoneNumber = "9876543210" };
            _repoMock.Setup(r => r.GetByEmailAsync(dto.Email)).ReturnsAsync((User)null!);
            _emailServiceMock.Setup(e => e.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            // Act
            await _authService.Register(dto);

            // Assert
            _memoryCache.TryGetValue($"RegistrationOtp_{dto.Email}", out string? otp);
            Assert.NotNull(otp);
            Assert.Equal(6, otp!.Length);
            Assert.True(int.TryParse(otp, out _));
        }

        // ── VerifyRegistrationOtp Tests ─────────────────────────────────────────

        [Fact]
        public async Task VerifyRegistrationOtp_InvalidOtp_ThrowsValidationException()
        {
            // Arrange
            var email = "verify@test.com";
            _memoryCache.Set($"RegistrationOtp_{email}", "123456", TimeSpan.FromMinutes(10));
            _memoryCache.Set($"RegistrationData_{email}", new RegisterDTO { Email = email }, TimeSpan.FromMinutes(10));

            var dto = new VerifyOtpDTO { Email = email, Otp = "999999" };

            // Act & Assert
            await Assert.ThrowsAsync<ValidationException>(() => _authService.VerifyRegistrationOtp(dto));
        }

        [Fact]
        public async Task VerifyRegistrationOtp_ExpiredOtp_ThrowsValidationException()
        {
            // Arrange - no otp in cache (simulates expiry)
            var dto = new VerifyOtpDTO { Email = "expired@test.com", Otp = "123456" };

            // Act & Assert
            await Assert.ThrowsAsync<ValidationException>(() => _authService.VerifyRegistrationOtp(dto));
        }

        [Fact]
        public async Task VerifyRegistrationOtp_ValidOtp_CreatesUserAndPublishesEvent()
        {
            // Arrange
            var email  = "valid@test.com";
            var otp    = "654321";
            var regDto = new RegisterDTO
            {
                Email       = email,
                FullName    = "Valid User",
                Password    = "SecurePassword!",
                PhoneNumber = "9876543210"
            };

            _memoryCache.Set($"RegistrationOtp_{email}",  otp,    TimeSpan.FromMinutes(10));
            _memoryCache.Set($"RegistrationData_{email}", regDto, TimeSpan.FromMinutes(10));

            User? savedUser = null;
            _repoMock.Setup(r => r.AddAsync(It.IsAny<User>()))
                .Callback<User>(u => savedUser = u)
                .Returns(Task.CompletedTask);
            _repoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

            _emailServiceMock.Setup(e => e.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            var verifyDto = new VerifyOtpDTO { Email = email, Otp = otp };

            // Act
            var result = await _authService.VerifyRegistrationOtp(verifyDto);

            // Assert
            Assert.Contains("successful", result);
            _repoMock.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Once);
            _repoMock.Verify(r => r.SaveChangesAsync(),          Times.Once);
            _busMock.Verify(b  => b.Publish(It.IsAny<SmartSure.Shared.Contracts.Events.UserRegisteredEvent>(), default), Times.Once);

            Assert.NotNull(savedUser);
            Assert.Equal(email,   savedUser!.Email);
            Assert.Equal("Valid User", savedUser.FullName);
            Assert.True(savedUser.IsEmailVerified);

            // OTP and data should be removed from cache after verification
            Assert.False(_memoryCache.TryGetValue($"RegistrationOtp_{email}",  out _));
            Assert.False(_memoryCache.TryGetValue($"RegistrationData_{email}", out _));
        }

        // ── Login Tests ─────────────────────────────────────────────────────────

        [Fact]
        public async Task Login_UserNotFound_ThrowsUnauthorizedException()
        {
            // Arrange
            var dto = new LoginDTO { Email = "notfound@test.com", Password = "any" };
            _repoMock.Setup(r => r.GetByEmailAsync(dto.Email)).ReturnsAsync((User)null!);

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedException>(() => _authService.Login(dto));
        }

        [Fact]
        public async Task Login_WrongPassword_ThrowsUnauthorizedException()
        {
            // Arrange
            var user = CreateUser();
            var dto  = new LoginDTO { Email = user.Email, Password = "WrongPassword" };
            _repoMock.Setup(r => r.GetByEmailAsync(dto.Email)).ReturnsAsync(user);

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedException>(() => _authService.Login(dto));
        }

        [Fact]
        public async Task Login_ValidCredentials_ReturnsTokenResponse()
        {
            // Arrange
            const string password = "Password123!";
            var user = CreateUser(password: password);
            var dto  = new LoginDTO { Email = user.Email, Password = password };

            _repoMock.Setup(r => r.GetByEmailAsync(dto.Email)).ReturnsAsync(user);
            _tokenServiceMock.Setup(t => t.BuildToken(
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<List<string>>()))
                .Returns("jwt-token");
            _tokenServiceMock.Setup(t => t.GenerateRefreshToken()).Returns("refresh-token");

            // Act
            var result = await _authService.Login(dto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("jwt-token",     result.Token);
            Assert.Equal("refresh-token", result.RefreshToken);
            Assert.Equal("Customer",      result.Role);
        }

        [Fact]
        public async Task Login_ValidCredentials_RefreshTokenCachedFor24Hours()
        {
            // Arrange
            const string password = "Password123!";
            var user = CreateUser(password: password);
            var dto  = new LoginDTO { Email = user.Email, Password = password };

            _repoMock.Setup(r => r.GetByEmailAsync(dto.Email)).ReturnsAsync(user);
            _tokenServiceMock.Setup(t => t.BuildToken(
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<List<string>>()))
                .Returns("jwt-token");
            _tokenServiceMock.Setup(t => t.GenerateRefreshToken()).Returns("myrefresh");

            // Act
            await _authService.Login(dto);

            // Assert - refresh token is in cache
            Assert.True(_memoryCache.TryGetValue("refreshToken_myrefresh", out string? cachedUserId));
            Assert.Equal(user.UserId.ToString(), cachedUserId);
        }

        // ── Refresh Token Tests ─────────────────────────────────────────────────

        [Fact]
        public async Task Refresh_InvalidToken_ThrowsUnauthorizedException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedException>(() => _authService.Refresh("nonexistent-token"));
        }

        [Fact]
        public async Task Refresh_ValidToken_ReturnsNewTokens()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user   = CreateUser();
            user.UserId = userId;

            const string oldRefresh = "old-refresh-token";
            _memoryCache.Set($"refreshToken_{oldRefresh}", userId.ToString(), TimeSpan.FromHours(24));

            _repoMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);
            _tokenServiceMock.Setup(t => t.BuildToken(
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<List<string>>()))
                .Returns("new-jwt-token");
            _tokenServiceMock.Setup(t => t.GenerateRefreshToken()).Returns("new-refresh-token");

            // Act
            var result = await _authService.Refresh(oldRefresh);

            // Assert
            Assert.Equal("new-jwt-token",     result.Token);
            Assert.Equal("new-refresh-token", result.RefreshToken);
            // Old token should be revoked
            Assert.False(_memoryCache.TryGetValue($"refreshToken_{oldRefresh}", out _));
            // New token should be cached
            Assert.True(_memoryCache.TryGetValue("refreshToken_new-refresh-token", out _));
        }

        // ── Password Management Tests ───────────────────────────────────────────

        [Fact]
        public async Task ChangePassword_UserNotFound_ThrowsNotFoundException()
        {
            // Arrange
            var userId = Guid.NewGuid();
            _repoMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync((User)null!);
            var dto = new ChangePasswordDTO { OldPassword = "old", NewPassword = "new" };

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() => _authService.ChangePassword(userId.ToString(), dto));
        }

        [Fact]
        public async Task ChangePassword_WrongOldPassword_ThrowsValidationException()
        {
            // Arrange
            var user = CreateUser(password: "CorrectOldPass!");
            _repoMock.Setup(r => r.GetByIdAsync(user.UserId)).ReturnsAsync(user);
            var dto = new ChangePasswordDTO { OldPassword = "WrongOldPass!", NewPassword = "NewPass!" };

            // Act & Assert
            await Assert.ThrowsAsync<ValidationException>(() => _authService.ChangePassword(user.UserId.ToString(), dto));
        }

        [Fact]
        public async Task ChangePassword_CorrectOldPassword_UpdatesPasswordHash()
        {
            // Arrange
            const string oldPassword = "OldPassword123!";
            const string newPassword = "NewPassword456!";
            var user = CreateUser(password: oldPassword);

            _repoMock.Setup(r => r.GetByIdAsync(user.UserId)).ReturnsAsync(user);
            _repoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

            var dto = new ChangePasswordDTO { OldPassword = oldPassword, NewPassword = newPassword };

            // Act
            await _authService.ChangePassword(user.UserId.ToString(), dto);

            // Assert
            _repoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
            // New password hash must verify correctly
            Assert.True(PasswordHasher.Verify(newPassword, user.Password.PasswordHash));
        }

        [Fact]
        public async Task ResetPassword_InvalidOtp_ThrowsValidationException()
        {
            // Arrange
            _otpServiceMock.Setup(o => o.ValidateOtpAsync("test@test.com", "badotp")).ReturnsAsync(false);
            var dto = new ResetPasswordWithOtpDTO { Email = "test@test.com", Otp = "badotp", NewPassword = "new" };

            // Act & Assert
            await Assert.ThrowsAsync<ValidationException>(() => _authService.ResetPasswordAsync(dto));
        }

        [Fact]
        public async Task ResetPassword_ValidOtp_UpdatesPasswordHash()
        {
            // Arrange
            const string email       = "reset@test.com";
            const string newPassword = "BrandNewPass!";
            var user = CreateUser(email: email);

            _otpServiceMock.Setup(o => o.ValidateOtpAsync(email, "validotp")).ReturnsAsync(true);
            _repoMock.Setup(r => r.GetByEmailAsync(email)).ReturnsAsync(user);
            _repoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

            var dto = new ResetPasswordWithOtpDTO { Email = email, Otp = "validotp", NewPassword = newPassword };

            // Act
            await _authService.ResetPasswordAsync(dto);

            // Assert
            _repoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
            Assert.True(PasswordHasher.Verify(newPassword, user.Password.PasswordHash));
        }

        // ── Profile Tests ───────────────────────────────────────────────────────

        [Fact]
        public async Task GetProfile_UserNotFound_ThrowsNotFoundException()
        {
            // Arrange
            var userId = Guid.NewGuid();
            _repoMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync((User)null!);

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() => _authService.GetProfile(userId.ToString()));
        }

        [Fact]
        public async Task GetProfile_UserExists_ReturnsCorrectDTO()
        {
            // Arrange
            var user = CreateUser();
            _repoMock.Setup(r => r.GetByIdAsync(user.UserId)).ReturnsAsync(user);

            // Act
            var result = await _authService.GetProfile(user.UserId.ToString());

            // Assert
            Assert.NotNull(result);
            Assert.Equal(user.UserId,      result.UserId);
            Assert.Equal(user.Email,       result.Email);
            Assert.Equal(user.FullName,    result.FullName);
            Assert.Equal(user.PhoneNumber, result.PhoneNumber);
        }

        [Fact]
        public async Task GetAllUsers_ReturnsListOfUserDTOs()
        {
            // Arrange
            var users = new List<User> { CreateUser("a@test.com"), CreateUser("b@test.com") };
            _repoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(users);

            // Act
            var result = await _authService.GetAllUsers();

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Contains(result, u => u.Email == "a@test.com");
            Assert.Contains(result, u => u.Email == "b@test.com");
        }

        [Fact]
        public async Task UpdateProfile_UserNotFound_ThrowsNotFoundException()
        {
            // Arrange
            var userId = Guid.NewGuid();
            _repoMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync((User)null!);
            var dto = new UpdateUserDTO { FullName = "X", PhoneNumber = "123", Address = "Y" };

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() => _authService.UpdateProfile(userId.ToString(), dto));
        }

        [Fact]
        public async Task UpdateProfile_UserExists_UpdatesFieldsAndSaves()
        {
            // Arrange
            var user = CreateUser();
            _repoMock.Setup(r => r.GetByIdAsync(user.UserId)).ReturnsAsync(user);
            _repoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

            var dto = new UpdateUserDTO { FullName = "Updated Name", PhoneNumber = "1111111111", Address = "456 New St" };

            // Act
            await _authService.UpdateProfile(user.UserId.ToString(), dto);

            // Assert
            Assert.Equal("Updated Name",  user.FullName);
            Assert.Equal("1111111111",    user.PhoneNumber);
            Assert.Equal("456 New St",    user.Address);
            _repoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }
    }
}
