using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using SmartSure.ClaimsService.Data;
using SmartSure.ClaimsService.DTOs;
using SmartSure.ClaimsService.Models;
using SmartSure.ClaimsService.Services;
using SmartSure.Shared.Contracts.Constants;
using SmartSure.Shared.Contracts.Exceptions;
using System;
using System.Threading.Tasks;
using Xunit;

namespace SmartSure.ClaimsService.Tests
{
    public class ClaimServiceTests
    {
        private ClaimsDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<ClaimsDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new ClaimsDbContext(options);
        }

        [Fact]
        public async Task CreateClaim_NoPendingClaims_CreatesDraftClaim()
        {
            // Arrange
            var dbContext = GetDbContext();
            var busMock = new Mock<IBus>();
            var loggerMock = new Mock<ILogger<ClaimService>>();
            var claimService = new ClaimService(dbContext, busMock.Object, loggerMock.Object);

            var userId = Guid.NewGuid();
            var dto = new CreateClaimDTO
            {
                PolicyId = Guid.NewGuid(),
                ClaimAmount = 50000,
                Description = "Accident",
                ClaimType = "OwnDamage",
                IsCompletelyDamaged = false
            };

            // Act
            var result = await claimService.CreateClaimAsync(userId, dto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(ClaimStatus.Draft, result.Status);
            Assert.Equal(dto.ClaimAmount, result.ClaimAmount);
        }

        [Fact]
        public async Task CreateClaim_ExistingPendingClaim_ThrowsConflictException()
        {
            // Arrange
            var dbContext = GetDbContext();
            var busMock = new Mock<IBus>();
            var loggerMock = new Mock<ILogger<ClaimService>>();
            var claimService = new ClaimService(dbContext, busMock.Object, loggerMock.Object);

            var userId = Guid.NewGuid();
            var policyId = Guid.NewGuid();
            
            // Add existing under-review claim
            dbContext.Claims.Add(new Claim
            {
                ClaimId = Guid.NewGuid(),
                PolicyId = policyId,
                UserId = userId,
                Status = ClaimStatus.UnderReview,
                Description = "Previous claim"
            });
            await dbContext.SaveChangesAsync();

            var dto = new CreateClaimDTO
            {
                PolicyId = policyId,
                ClaimAmount = 50000,
                Description = "Accident"
            };

            // Act & Assert
            await Assert.ThrowsAsync<ConflictException>(() => claimService.CreateClaimAsync(userId, dto));
        }

        [Fact]
        public async Task SubmitClaim_DraftClaim_UpdatesStatusAndPublishesEvent()
        {
            // Arrange
            var dbContext = GetDbContext();
            var busMock = new Mock<IBus>();
            var loggerMock = new Mock<ILogger<ClaimService>>();
            var claimService = new ClaimService(dbContext, busMock.Object, loggerMock.Object);

            var claimId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            
            var claim = new Claim
            {
                ClaimId = claimId,
                PolicyId = Guid.NewGuid(),
                UserId = userId,
                Status = ClaimStatus.Draft,
                Description = "Draft claim"
            };
            dbContext.Claims.Add(claim);
            await dbContext.SaveChangesAsync();

            // Act
            await claimService.SubmitClaimAsync(claimId, userId);

            // Assert
            var updatedClaim = await dbContext.Claims.FindAsync(claimId);
            Assert.Equal(ClaimStatus.Submitted, updatedClaim.Status);
            busMock.Verify(b => b.Publish(It.IsAny<SmartSure.Shared.Contracts.Events.ClaimSubmittedEvent>(), default), Times.Once);
        }
    }
}
