using IdentityService.Models;
using IdentityService.Repositories;
using IdentityService.Services;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace SmartSure.IdentityService.Tests
{
    /// <summary>
    /// Unit tests for OtpService - covering OTP generation, validation rules,
    /// attempt limits, and expiry enforcement.
    /// </summary>
    public class OtpServiceTests
    {
        private readonly Mock<IUserRepository> _userRepoMock;
        private readonly Mock<IOtpRepository> _otpRepoMock;
        private readonly Mock<IEmailService> _emailServiceMock;
        private readonly OtpService _otpService;

        public OtpServiceTests()
        {
            _userRepoMock    = new Mock<IUserRepository>();
            _otpRepoMock     = new Mock<IOtpRepository>();
            _emailServiceMock = new Mock<IEmailService>();

            _otpService = new OtpService(_userRepoMock.Object, _otpRepoMock.Object, _emailServiceMock.Object);
        }

        // ── GenerateAndSendOtpAsync Tests ───────────────────────────────────────

        [Fact]
        public async Task GenerateAndSendOtp_UserNotFound_ThrowsException()
        {
            // Arrange
            const string email = "notfound@test.com";
            _userRepoMock.Setup(r => r.GetByEmailAsync(email)).ReturnsAsync((User)null!);

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _otpService.GenerateAndSendOtpAsync(email));
        }

        [Fact]
        public async Task GenerateAndSendOtp_UserExists_DeletesOldOtpsAndCreatesNew()
        {
            // Arrange
            const string email = "user@test.com";
            var user = new User { UserId = Guid.NewGuid(), Email = email };
            var existingOtps = new List<OtpRecord>
            {
                new OtpRecord { UserId = user.UserId, Email = email, Otp = "111111" },
                new OtpRecord { UserId = user.UserId, Email = email, Otp = "222222" }
            };

            _userRepoMock.Setup(r => r.GetByEmailAsync(email)).ReturnsAsync(user);
            _otpRepoMock.Setup(r => r.GetAllByEmailAsync(email)).ReturnsAsync(existingOtps);
            _otpRepoMock.Setup(r => r.AddAsync(It.IsAny<OtpRecord>())).Returns(Task.CompletedTask);
            _otpRepoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);
            _emailServiceMock.Setup(e => e.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            // Act
            var generatedOtp = await _otpService.GenerateAndSendOtpAsync(email);

            // Assert
            _otpRepoMock.Verify(r => r.RemoveRangeAsync(existingOtps), Times.Once);
            _otpRepoMock.Verify(r => r.AddAsync(It.IsAny<OtpRecord>()), Times.Once);
            _otpRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
            _emailServiceMock.Verify(e => e.SendEmailAsync(email, It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            Assert.NotNull(generatedOtp);
            Assert.Equal(6, generatedOtp.Length);
        }

        [Fact]
        public async Task GenerateAndSendOtp_NoExistingOtps_SkipsRemoveRange()
        {
            // Arrange
            const string email = "fresh@test.com";
            var user = new User { UserId = Guid.NewGuid(), Email = email };

            _userRepoMock.Setup(r => r.GetByEmailAsync(email)).ReturnsAsync(user);
            _otpRepoMock.Setup(r => r.GetAllByEmailAsync(email)).ReturnsAsync(new List<OtpRecord>());
            _otpRepoMock.Setup(r => r.AddAsync(It.IsAny<OtpRecord>())).Returns(Task.CompletedTask);
            _otpRepoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);
            _emailServiceMock.Setup(e => e.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            // Act
            await _otpService.GenerateAndSendOtpAsync(email);

            // Assert - RemoveRangeAsync should NOT be called when no existing OTPs
            _otpRepoMock.Verify(r => r.RemoveRangeAsync(It.IsAny<List<OtpRecord>>()), Times.Never);
        }

        [Fact]
        public async Task GenerateAndSendOtp_GeneratedOtpIs6Digits()
        {
            // Arrange
            const string email = "six@test.com";
            var user = new User { UserId = Guid.NewGuid(), Email = email };

            _userRepoMock.Setup(r => r.GetByEmailAsync(email)).ReturnsAsync(user);
            _otpRepoMock.Setup(r => r.GetAllByEmailAsync(email)).ReturnsAsync(new List<OtpRecord>());
            _otpRepoMock.Setup(r => r.AddAsync(It.IsAny<OtpRecord>())).Returns(Task.CompletedTask);
            _otpRepoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);
            _emailServiceMock.Setup(e => e.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            // Act
            var otp = await _otpService.GenerateAndSendOtpAsync(email);

            // Assert
            Assert.Equal(6, otp.Length);
            Assert.True(int.TryParse(otp, out int numericOtp));
            Assert.True(numericOtp >= 100000 && numericOtp <= 999999);
        }

        // ── ValidateOtpAsync Tests ───────────────────────────────────────────────

        [Fact]
        public async Task ValidateOtp_NoRecord_ReturnsFalse()
        {
            // Arrange
            _otpRepoMock.Setup(r => r.GetByEmailAsync("notfound@test.com")).ReturnsAsync((OtpRecord)null!);

            // Act
            var result = await _otpService.ValidateOtpAsync("notfound@test.com", "123456");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task ValidateOtp_ExpiredOtp_ReturnsFalse()
        {
            // Arrange
            var record = new OtpRecord
            {
                Otp            = "123456",
                ExpirationTime = DateTime.UtcNow.AddMinutes(-5), // Already expired
                Attempts       = 0
            };
            _otpRepoMock.Setup(r => r.GetByEmailAsync("expired@test.com")).ReturnsAsync(record);

            // Act
            var result = await _otpService.ValidateOtpAsync("expired@test.com", "123456");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task ValidateOtp_ExceededAttempts_DeletesRecordAndReturnsFalse()
        {
            // Arrange
            var record = new OtpRecord
            {
                Otp            = "123456",
                ExpirationTime = DateTime.UtcNow.AddMinutes(5),
                Attempts       = 3 // Already at max
            };
            _otpRepoMock.Setup(r => r.GetByEmailAsync("toomany@test.com")).ReturnsAsync(record);
            _otpRepoMock.Setup(r => r.RemoveAsync(record)).Returns(Task.CompletedTask);
            _otpRepoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

            // Act
            var result = await _otpService.ValidateOtpAsync("toomany@test.com", "123456");

            // Assert
            Assert.False(result);
            _otpRepoMock.Verify(r => r.RemoveAsync(record), Times.Once);
            _otpRepoMock.Verify(r => r.SaveChangesAsync(),  Times.Once);
        }

        [Fact]
        public async Task ValidateOtp_WrongOtp_IncrementsAttemptAndReturnsFalse()
        {
            // Arrange
            var record = new OtpRecord
            {
                Otp            = "999999",
                ExpirationTime = DateTime.UtcNow.AddMinutes(5),
                Attempts       = 1
            };
            _otpRepoMock.Setup(r => r.GetByEmailAsync("wrong@test.com")).ReturnsAsync(record);
            _otpRepoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

            // Act
            var result = await _otpService.ValidateOtpAsync("wrong@test.com", "111111");

            // Assert
            Assert.False(result);
            Assert.Equal(2, record.Attempts); // Incremented from 1 to 2
            _otpRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task ValidateOtp_CorrectOtp_DeletesRecordAndReturnsTrue()
        {
            // Arrange
            var record = new OtpRecord
            {
                Otp            = "654321",
                ExpirationTime = DateTime.UtcNow.AddMinutes(5),
                Attempts       = 0
            };
            _otpRepoMock.Setup(r => r.GetByEmailAsync("correct@test.com")).ReturnsAsync(record);
            _otpRepoMock.Setup(r => r.RemoveAsync(record)).Returns(Task.CompletedTask);
            _otpRepoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

            // Act
            var result = await _otpService.ValidateOtpAsync("correct@test.com", "654321");

            // Assert
            Assert.True(result);
            _otpRepoMock.Verify(r => r.RemoveAsync(record), Times.Once);
            _otpRepoMock.Verify(r => r.SaveChangesAsync(),  Times.Once);
        }

        [Fact]
        public async Task ValidateOtp_FirstAttemptWith2Attempts_DoesNotDelete()
        {
            // Arrange
            var record = new OtpRecord
            {
                Otp            = "777777",
                ExpirationTime = DateTime.UtcNow.AddMinutes(5),
                Attempts       = 2 // One more attempt left (2 < 3)
            };
            _otpRepoMock.Setup(r => r.GetByEmailAsync("last@test.com")).ReturnsAsync(record);
            _otpRepoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

            // Act
            var result = await _otpService.ValidateOtpAsync("last@test.com", "000000"); // Wrong

            // Assert
            Assert.False(result);
            Assert.Equal(3, record.Attempts); // Incremented to 3
            // RemoveAsync should NOT be called (we only call it when attempts >= 3 at the START)
            _otpRepoMock.Verify(r => r.RemoveAsync(It.IsAny<OtpRecord>()), Times.Never);
        }
    }
}
