using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using SmartSure.ClaimsService.DTOs;
using SmartSure.ClaimsService.Models;
using SmartSure.ClaimsService.Repositories;
using SmartSure.ClaimsService.Services;
using SmartSure.Shared.Contracts.Constants;
using SmartSure.Shared.Contracts.DTOs;
using SmartSure.Shared.Contracts.Events;
using SmartSure.Shared.Contracts.Exceptions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace SmartSure.ClaimsService.Tests
{
    /// <summary>
    /// Comprehensive unit tests for ClaimService - covering claim creation business rules,
    /// state machine transitions, approval/rejection workflows, and validation constraints.
    /// </summary>
    public class ClaimServiceFullTests
    {
        private readonly Mock<IClaimRepository>              _claimRepoMock;
        private readonly Mock<IClaimStatusHistoryRepository> _historyRepoMock;
        private readonly Mock<IBus>                         _busMock;
        private readonly Mock<ILogger<ClaimService>>        _loggerMock;
        private readonly Mock<IEmailService>                _emailServiceMock;
        private readonly Mock<IHttpClientFactory>           _httpClientFactoryMock;
        private readonly Mock<IConfiguration>               _configMock;
        private readonly Mock<IHttpContextAccessor>         _httpContextAccessorMock;
        private readonly ClaimService                       _service;

        public ClaimServiceFullTests()
        {
            _claimRepoMock           = new Mock<IClaimRepository>();
            _historyRepoMock         = new Mock<IClaimStatusHistoryRepository>();
            _busMock                 = new Mock<IBus>();
            _loggerMock              = new Mock<ILogger<ClaimService>>();
            _emailServiceMock        = new Mock<IEmailService>();
            _httpClientFactoryMock   = new Mock<IHttpClientFactory>();
            _configMock              = new Mock<IConfiguration>();
            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();

            _configMock.Setup(c => c["Gateway:Url"]).Returns("http://localhost:5057");

            // Set up a default null HttpClient (not used in most tests)
            var httpContext = new DefaultHttpContext();
            _httpContextAccessorMock.Setup(h => h.HttpContext).Returns(httpContext);

            _service = new ClaimService(
                _claimRepoMock.Object,
                _historyRepoMock.Object,
                _busMock.Object,
                _loggerMock.Object,
                _emailServiceMock.Object,
                _httpClientFactoryMock.Object,
                _configMock.Object,
                _httpContextAccessorMock.Object
            );
        }

        // ── Helpers ─────────────────────────────────────────────────────────────

        private static Claim MakeClaim(Guid? policyId = null, string status = ClaimStatus.Draft, string claimType = "OwnDamage")
        {
            return new Claim
            {
                ClaimId     = Guid.NewGuid(),
                PolicyId    = policyId ?? Guid.NewGuid(),
                UserId      = Guid.NewGuid(),
                Description = "Accident on highway",
                Status      = status,
                ClaimAmount = 50000m,
                ClaimType   = claimType
            };
        }

        private static PagedResult<Claim> CreatePagedResult(List<Claim> claims)
        {
            return new PagedResult<Claim>
            {
                Page       = 1,
                PageSize   = 10,
                TotalCount = claims.Count,
                Items      = claims
            };
        }

        // ── CreateClaim Tests ──────────────────────────────────────────────────

        [Fact]
        public async Task CreateClaim_NoPendingClaims_CreatesDraftClaim()
        {
            // Arrange
            var userId   = Guid.NewGuid();
            var policyId = Guid.NewGuid();
            _claimRepoMock.Setup(r => r.GetByPolicyIdAsync(policyId)).ReturnsAsync(new List<Claim>());
            _claimRepoMock.Setup(r => r.AddAsync(It.IsAny<Claim>())).Returns(Task.CompletedTask);
            _claimRepoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

            var dto = new CreateClaimDTO
            {
                PolicyId           = policyId,
                Description        = "Car accident",
                ClaimAmount        = 75000m,
                ClaimType          = "OwnDamage",
                IsCompletelyDamaged = false
            };

            // Act
            var result = await _service.CreateClaimAsync(userId, dto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(ClaimStatus.Draft, result.Status);
            Assert.Equal(75000m,            result.ClaimAmount);
            Assert.Equal(policyId,          result.PolicyId);
            _claimRepoMock.Verify(r => r.AddAsync(It.IsAny<Claim>()), Times.Once);
            _claimRepoMock.Verify(r => r.SaveChangesAsync(),           Times.Once);
        }

        [Fact]
        public async Task CreateClaim_ExistingDraftClaim_ThrowsConflictException()
        {
            // Arrange
            var policyId       = Guid.NewGuid();
            var existingClaims = new List<Claim> { MakeClaim(policyId, ClaimStatus.Draft) };
            _claimRepoMock.Setup(r => r.GetByPolicyIdAsync(policyId)).ReturnsAsync(existingClaims);

            var dto = new CreateClaimDTO { PolicyId = policyId, ClaimAmount = 10000m, Description = "New" };

            // Act & Assert
            await Assert.ThrowsAsync<ConflictException>(() => _service.CreateClaimAsync(Guid.NewGuid(), dto));
        }

        [Fact]
        public async Task CreateClaim_ExistingSubmittedClaim_ThrowsConflictException()
        {
            // Arrange
            var policyId       = Guid.NewGuid();
            var existingClaims = new List<Claim> { MakeClaim(policyId, ClaimStatus.Submitted) };
            _claimRepoMock.Setup(r => r.GetByPolicyIdAsync(policyId)).ReturnsAsync(existingClaims);

            var dto = new CreateClaimDTO { PolicyId = policyId, ClaimAmount = 10000m, Description = "New" };

            // Act & Assert
            await Assert.ThrowsAsync<ConflictException>(() => _service.CreateClaimAsync(Guid.NewGuid(), dto));
        }

        [Fact]
        public async Task CreateClaim_ExistingUnderReviewClaim_ThrowsConflictException()
        {
            // Arrange
            var policyId       = Guid.NewGuid();
            var existingClaims = new List<Claim> { MakeClaim(policyId, ClaimStatus.UnderReview) };
            _claimRepoMock.Setup(r => r.GetByPolicyIdAsync(policyId)).ReturnsAsync(existingClaims);

            var dto = new CreateClaimDTO { PolicyId = policyId, ClaimAmount = 10000m, Description = "Another" };

            // Act & Assert
            await Assert.ThrowsAsync<ConflictException>(() => _service.CreateClaimAsync(Guid.NewGuid(), dto));
        }

        [Fact]
        public async Task CreateClaim_MaxApprovedClaimsReached_ThrowsConflictException()
        {
            // Arrange
            var policyId = Guid.NewGuid();
            var existingClaims = new List<Claim>
            {
                MakeClaim(policyId, ClaimStatus.Approved),
                MakeClaim(policyId, ClaimStatus.Approved),
                MakeClaim(policyId, ClaimStatus.Approved)  // 3 approved = max limit
            };
            _claimRepoMock.Setup(r => r.GetByPolicyIdAsync(policyId)).ReturnsAsync(existingClaims);

            var dto = new CreateClaimDTO { PolicyId = policyId, ClaimAmount = 10000m, Description = "4th claim" };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<ConflictException>(() => _service.CreateClaimAsync(Guid.NewGuid(), dto));
            Assert.Contains("3 claims", ex.Message);
        }

        [Fact]
        public async Task CreateClaim_PreviousTheftClaimApproved_ThrowsConflictException()
        {
            // Arrange
            var policyId = Guid.NewGuid();
            var existingClaims = new List<Claim>
            {
                MakeClaim(policyId, ClaimStatus.Approved, "Stolen")  // Theft claim
            };
            _claimRepoMock.Setup(r => r.GetByPolicyIdAsync(policyId)).ReturnsAsync(existingClaims);

            var dto = new CreateClaimDTO { PolicyId = policyId, ClaimAmount = 10000m, Description = "New claim after theft" };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<ConflictException>(() => _service.CreateClaimAsync(Guid.NewGuid(), dto));
            Assert.Contains("terminated", ex.Message.ToLower());
        }

        [Fact]
        public async Task CreateClaim_OnlyRejectedPriorClaims_AllowsNewClaim()
        {
            // Arrange – rejected claims should not block new submissions
            var policyId       = Guid.NewGuid();
            var existingClaims = new List<Claim>
            {
                MakeClaim(policyId, ClaimStatus.Rejected),
                MakeClaim(policyId, ClaimStatus.Closed)
            };
            _claimRepoMock.Setup(r => r.GetByPolicyIdAsync(policyId)).ReturnsAsync(existingClaims);
            _claimRepoMock.Setup(r => r.AddAsync(It.IsAny<Claim>())).Returns(Task.CompletedTask);
            _claimRepoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

            var dto = new CreateClaimDTO { PolicyId = policyId, ClaimAmount = 10000m, Description = "New valid claim" };

            // Act
            var result = await _service.CreateClaimAsync(Guid.NewGuid(), dto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(ClaimStatus.Draft, result.Status);
        }

        // ── SubmitClaim Tests ──────────────────────────────────────────────────

        [Fact]
        public async Task SubmitClaim_DraftClaim_TransitionsToSubmittedAndPublishesEvents()
        {
            // Arrange
            var claim = MakeClaim(status: ClaimStatus.Draft);
            _claimRepoMock.Setup(r => r.GetByIdAsync(claim.ClaimId)).ReturnsAsync(claim);
            _claimRepoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

            // Act
            await _service.SubmitClaimAsync(claim.ClaimId, claim.UserId);

            // Assert
            Assert.Equal(ClaimStatus.Submitted, claim.Status);
            _claimRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
            _busMock.Verify(b => b.Publish(It.IsAny<ClaimSubmittedEvent>(), default),       Times.Once);
            _busMock.Verify(b => b.Publish(It.IsAny<ClaimStatusChangedEvent>(), default), Times.Once);
        }

        [Fact]
        public async Task SubmitClaim_NonDraftClaim_ThrowsBusinessRuleException()
        {
            // Arrange
            var claim = MakeClaim(status: ClaimStatus.Submitted);
            _claimRepoMock.Setup(r => r.GetByIdAsync(claim.ClaimId)).ReturnsAsync(claim);

            // Act & Assert
            await Assert.ThrowsAsync<BusinessRuleException>(() => _service.SubmitClaimAsync(claim.ClaimId, Guid.NewGuid()));
        }

        [Fact]
        public async Task SubmitClaim_ClaimNotFound_ThrowsNotFoundException()
        {
            // Arrange
            var claimId = Guid.NewGuid();
            _claimRepoMock.Setup(r => r.GetByIdAsync(claimId)).ReturnsAsync((Claim)null!);

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() => _service.SubmitClaimAsync(claimId, Guid.NewGuid()));
        }

        [Fact]
        public async Task SubmitClaim_AddsStatusHistoryEntry()
        {
            // Arrange
            var claim = MakeClaim(status: ClaimStatus.Draft);
            _claimRepoMock.Setup(r => r.GetByIdAsync(claim.ClaimId)).ReturnsAsync(claim);
            _claimRepoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

            // Act
            await _service.SubmitClaimAsync(claim.ClaimId, claim.UserId);

            // Assert
            Assert.Single(claim.StatusHistory);
            var historyEntry = Assert.Single(claim.StatusHistory);
            Assert.Equal(ClaimStatus.Draft,     historyEntry.OldStatus);
            Assert.Equal(ClaimStatus.Submitted, historyEntry.NewStatus);
        }

        // ── UpdateClaim Tests ──────────────────────────────────────────────────

        [Fact]
        public async Task UpdateClaim_DraftClaim_UpdatesFields()
        {
            // Arrange
            var claim = MakeClaim(status: ClaimStatus.Draft);
            _claimRepoMock.Setup(r => r.GetByIdAsync(claim.ClaimId)).ReturnsAsync(claim);
            _claimRepoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

            var dto = new UpdateClaimDTO { Description = "Updated description", ClaimAmount = 99000m };

            // Act
            var result = await _service.UpdateClaimAsync(claim.ClaimId, dto);

            // Assert
            Assert.Equal("Updated description", result.Description);
            Assert.Equal(99000m,               result.ClaimAmount);
            _claimRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateClaim_NonDraftClaim_ThrowsBusinessRuleException()
        {
            // Arrange
            var claim = MakeClaim(status: ClaimStatus.Submitted);
            _claimRepoMock.Setup(r => r.GetByIdAsync(claim.ClaimId)).ReturnsAsync(claim);

            // Act & Assert
            await Assert.ThrowsAsync<BusinessRuleException>(() => _service.UpdateClaimAsync(claim.ClaimId, new UpdateClaimDTO()));
        }

        [Fact]
        public async Task UpdateClaim_ClaimNotFound_ThrowsNotFoundException()
        {
            // Arrange
            var claimId = Guid.NewGuid();
            _claimRepoMock.Setup(r => r.GetByIdAsync(claimId)).ReturnsAsync((Claim)null!);

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() => _service.UpdateClaimAsync(claimId, new UpdateClaimDTO()));
        }

        [Fact]
        public async Task UpdateClaim_OnlyDescriptionProvided_UpdatesOnlyDescription()
        {
            // Arrange
            var claim = MakeClaim(status: ClaimStatus.Draft);
            claim.ClaimAmount = 50000m; // original amount
            _claimRepoMock.Setup(r => r.GetByIdAsync(claim.ClaimId)).ReturnsAsync(claim);
            _claimRepoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

            var dto = new UpdateClaimDTO { Description = "New description only", ClaimAmount = null };

            // Act
            var result = await _service.UpdateClaimAsync(claim.ClaimId, dto);

            // Assert
            Assert.Equal("New description only", result.Description);
            Assert.Equal(50000m,                 result.ClaimAmount); // unchanged
        }

        // ── UpdateClaimStatus Tests ────────────────────────────────────────────

        [Fact]
        public async Task UpdateClaimStatus_ClaimNotFound_ThrowsNotFoundException()
        {
            // Arrange
            var claimId = Guid.NewGuid();
            _claimRepoMock.Setup(r => r.GetByIdAsync(claimId)).ReturnsAsync((Claim)null!);

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() =>
                _service.UpdateClaimStatusAsync(claimId, ClaimStatus.UnderReview, "Note", "admin"));
        }

        [Fact]
        public async Task UpdateClaimStatus_ValidClaim_UpdatesStatusAndPublishesEvent()
        {
            // Arrange
            var claim = MakeClaim(status: ClaimStatus.Submitted);
            _claimRepoMock.Setup(r => r.GetByIdAsync(claim.ClaimId)).ReturnsAsync(claim);
            _claimRepoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

            // Act
            await _service.UpdateClaimStatusAsync(claim.ClaimId, ClaimStatus.UnderReview, "Under review now", "admin1");

            // Assert
            Assert.Equal(ClaimStatus.UnderReview, claim.Status);
            _claimRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
            _busMock.Verify(b => b.Publish(It.IsAny<ClaimStatusChangedEvent>(), default), Times.Once);
        }

        // ── RejectClaim Tests ──────────────────────────────────────────────────

        [Fact]
        public async Task RejectClaim_ClaimNotFound_ThrowsNotFoundException()
        {
            // Arrange
            var claimId = Guid.NewGuid();
            _claimRepoMock.Setup(r => r.GetByIdAsync(claimId)).ReturnsAsync((Claim)null!);

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() =>
                _service.RejectClaimAsync(claimId, "Insufficient evidence", "admin"));
        }

        [Fact]
        public async Task RejectClaim_ValidClaim_SetsStatusRejectedWithReason()
        {
            // Arrange
            var claim = MakeClaim(status: ClaimStatus.Submitted);
            _claimRepoMock.Setup(r => r.GetByIdAsync(claim.ClaimId)).ReturnsAsync(claim);
            _claimRepoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

            // Act
            await _service.RejectClaimAsync(claim.ClaimId, "False claim", "admin1");

            // Assert
            Assert.Equal(ClaimStatus.Rejected, claim.Status);
            Assert.Equal("False claim",        claim.RejectionReason);
            _claimRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
            _busMock.Verify(b => b.Publish(It.IsAny<ClaimStatusChangedEvent>(), default), Times.Once);
        }

        [Fact]
        public async Task RejectClaim_AddsStatusHistoryEntry()
        {
            // Arrange
            var claim = MakeClaim(status: ClaimStatus.Submitted);
            _claimRepoMock.Setup(r => r.GetByIdAsync(claim.ClaimId)).ReturnsAsync(claim);
            _claimRepoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

            // Act
            await _service.RejectClaimAsync(claim.ClaimId, "No docs", "admin2");

            // Assert
            Assert.Single(claim.StatusHistory);
            var history = Assert.Single(claim.StatusHistory);
            Assert.Equal(ClaimStatus.Submitted, history.OldStatus);
            Assert.Equal(ClaimStatus.Rejected,  history.NewStatus);
            Assert.Equal("No docs",             history.Notes);
        }

        // ── ApproveClaim Tests ─────────────────────────────────────────────────

        [Fact]
        public async Task ApproveClaim_ClaimNotFound_ThrowsNotFoundException()
        {
            // Arrange
            var claimId = Guid.NewGuid();
            _claimRepoMock.Setup(r => r.GetByIdAsync(claimId)).ReturnsAsync((Claim)null!);

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() =>
                _service.ApproveClaimAsync(claimId, 50000m, "Approved", "admin"));
        }

        [Fact]
        public async Task ApproveClaim_NonTheftClaim_SetsApprovedAmountAndPublishesEvent()
        {
            // Arrange
            var claim = MakeClaim(status: ClaimStatus.Submitted, claimType: "OwnDamage");
            _claimRepoMock.Setup(r => r.GetByIdAsync(claim.ClaimId)).ReturnsAsync(claim);
            _claimRepoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

            // Act
            await _service.ApproveClaimAsync(claim.ClaimId, 40000m, "Approved with partial amount", "admin");

            // Assert
            Assert.Equal(ClaimStatus.Approved, claim.Status);
            Assert.Equal(40000m,               claim.ApprovedAmount);
            _claimRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
            _busMock.Verify(b => b.Publish(It.IsAny<ClaimStatusChangedEvent>(), default), Times.Once);
        }

        [Fact]
        public async Task ApproveClaim_AddsStatusHistoryEntryWithAdminId()
        {
            // Arrange
            var claim   = MakeClaim(status: ClaimStatus.Submitted, claimType: "OwnDamage");
            var adminId = "admin-guid-123";

            _claimRepoMock.Setup(r => r.GetByIdAsync(claim.ClaimId)).ReturnsAsync(claim);
            _claimRepoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

            // Act
            await _service.ApproveClaimAsync(claim.ClaimId, 30000m, "All docs verified", adminId);

            // Assert
            Assert.Single(claim.StatusHistory);
            var history = Assert.Single(claim.StatusHistory);
            Assert.Equal(ClaimStatus.Approved, history.NewStatus);
            Assert.Equal(adminId,              history.ChangedBy);
            Assert.Equal("All docs verified",  history.Notes);
        }

        // ── TransitionStatus Tests ─────────────────────────────────────────────

        [Fact]
        public async Task TransitionStatus_ClaimNotFound_ThrowsNotFoundException()
        {
            // Arrange
            var claimId = Guid.NewGuid();
            _claimRepoMock.Setup(r => r.GetByIdAsync(claimId)).ReturnsAsync((Claim)null!);

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() =>
                _service.TransitionStatusAsync(claimId, ClaimStatus.Closed, "admin", "Case closed"));
        }

        [Fact]
        public async Task TransitionStatus_ValidClaim_UpdatesStatusAndPublishesEvent()
        {
            // Arrange
            var claim = MakeClaim(status: ClaimStatus.Approved);
            _claimRepoMock.Setup(r => r.GetByIdAsync(claim.ClaimId)).ReturnsAsync(claim);
            _claimRepoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

            // Act
            await _service.TransitionStatusAsync(claim.ClaimId, ClaimStatus.Closed, "admin", "Payment completed");

            // Assert
            Assert.Equal(ClaimStatus.Closed, claim.Status);
            Assert.Single(claim.StatusHistory);
            _claimRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
            _busMock.Verify(b => b.Publish(It.IsAny<ClaimStatusChangedEvent>(), default), Times.Once);
        }

        // ── GetClaim Tests ────────────────────────────────────────────────────

        [Fact]
        public async Task GetClaimById_NotFound_ReturnsNull()
        {
            // Arrange
            var claimId = Guid.NewGuid();
            _claimRepoMock.Setup(r => r.GetByIdAsync(claimId)).ReturnsAsync((Claim)null!);

            // Act
            var result = await _service.GetClaimByIdAsync(claimId);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetClaimById_Found_ReturnsMappedDTO()
        {
            // Arrange
            var claim = MakeClaim();
            _claimRepoMock.Setup(r => r.GetByIdAsync(claim.ClaimId)).ReturnsAsync(claim);

            // Act
            var result = await _service.GetClaimByIdAsync(claim.ClaimId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(claim.ClaimId,     result!.ClaimId);
            Assert.Equal(claim.ClaimAmount, result.ClaimAmount);
            Assert.Equal(claim.Status,      result.Status);
        }

        [Fact]
        public async Task GetClaimsByPolicy_ReturnsMappedList()
        {
            // Arrange
            var policyId = Guid.NewGuid();
            var claims   = new List<Claim> { MakeClaim(policyId), MakeClaim(policyId) };
            _claimRepoMock.Setup(r => r.GetByPolicyIdAsync(policyId)).ReturnsAsync(claims);

            // Act
            var result = await _service.GetClaimsByPolicyAsync(policyId);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.All(result, r => Assert.Equal(policyId, r.PolicyId));
        }

        [Fact]
        public async Task GetUserClaims_ReturnsPaged()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var claims = new List<Claim> { MakeClaim() };
            var paged  = CreatePagedResult(claims);
            _claimRepoMock.Setup(r => r.GetByUserIdAsync(userId, 1, 10)).ReturnsAsync(paged);

            // Act
            var result = await _service.GetUserClaimsAsync(userId, 1, 10);

            // Assert
            Assert.Equal(1, result.TotalCount);
            Assert.Single(result.Items);
        }

        // ── CreateClaim: Status History Population ─────────────────────────────

        [Fact]
        public async Task CreateClaim_InitialStatusHistory_ContainsDraftEntry()
        {
            // Arrange
            var userId   = Guid.NewGuid();
            var policyId = Guid.NewGuid();
            _claimRepoMock.Setup(r => r.GetByPolicyIdAsync(policyId)).ReturnsAsync(new List<Claim>());

            Claim? savedClaim = null;
            _claimRepoMock.Setup(r => r.AddAsync(It.IsAny<Claim>()))
                .Callback<Claim>(c => savedClaim = c)
                .Returns(Task.CompletedTask);
            _claimRepoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

            var dto = new CreateClaimDTO { PolicyId = policyId, ClaimAmount = 5000m, Description = "Test" };

            // Act
            await _service.CreateClaimAsync(userId, dto);

            // Assert
            Assert.NotNull(savedClaim);
            Assert.Single(savedClaim!.StatusHistory);
            var history = Assert.Single(savedClaim.StatusHistory);
            Assert.Equal(ClaimStatus.Draft, history.NewStatus);
            Assert.Equal("Claim created",   history.Notes);
        }
    }
}
