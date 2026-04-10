using MassTransit;
using Microsoft.Extensions.Logging;
using Moq;
using SmartSure.PolicyService.DTOs;
using SmartSure.PolicyService.Models;
using SmartSure.PolicyService.Repositories;
using SmartSure.PolicyService.Services;
using SmartSure.Shared.Contracts.Exceptions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace SmartSure.PolicyService.Tests
{
    public class PolicyMgmtServiceTests
    {
        private readonly Mock<IPolicyRepository> _policyRepoMock;
        private readonly Mock<IInsuranceRepository> _insuranceRepoMock;
        private readonly Mock<IBus> _busMock;
        private readonly Mock<IDiscountService> _discountServiceMock;
        private readonly Mock<ILogger<PolicyMgmtService>> _loggerMock;
        private readonly PolicyMgmtService _policyService;

        public PolicyMgmtServiceTests()
        {
            _policyRepoMock = new Mock<IPolicyRepository>();
            _insuranceRepoMock = new Mock<IInsuranceRepository>();
            _busMock = new Mock<IBus>();
            _discountServiceMock = new Mock<IDiscountService>();
            _loggerMock = new Mock<ILogger<PolicyMgmtService>>();
            
            _policyService = new PolicyMgmtService(
                _policyRepoMock.Object,
                _insuranceRepoMock.Object,
                _busMock.Object,
                _discountServiceMock.Object,
                _loggerMock.Object
            );
        }

        [Fact]
        public async Task CalculateQuote_VehicleInsurance_CalculatesCorrectPremium()
        {
            // Arrange
            var createDto = new CreatePolicyDTO
            {
                SubtypeId = Guid.NewGuid(),
                Duration = 12, // 1 year
                VehicleDetail = new PolicyVehicleDetailDTO
                {
                    EstimatedValue = 500000,
                    ManufactureYear = DateTime.UtcNow.Year // New car, 5% depreciation
                }
            };

            var subtype = new InsuranceSubtype { SubtypeId = createDto.SubtypeId, TypeId = Guid.NewGuid(), BasePremium = 2000, Name = "Comprehensive" };
            var type = new InsuranceType { TypeId = subtype.TypeId, Name = "Vehicle Insurance" };

            _insuranceRepoMock.Setup(r => r.GetSubtypeByIdAsync(createDto.SubtypeId)).ReturnsAsync(subtype);
            _insuranceRepoMock.Setup(r => r.GetTypeByIdAsync(subtype.TypeId)).ReturnsAsync(type);

            // Act
            var result = await _policyService.CalculateQuoteAsync(createDto);

            // Assert
            // IDV = 500,000 * 0.95 = 475,000
            // Base Premium = 2000 * 1 + (475,000 * 0.025 * 1) = 13875
            // GST (18%) = 13875 * 0.18 = 2497.5
            // Total Premium = 13875 + 2497.5 = 16372.5
            Assert.Equal(475000m, result.InsuredDeclaredValue);
            Assert.Equal(16372.50m, result.PremiumAmount);
        }

        [Fact]
        public async Task CalculateQuote_SubtypeNotFound_ThrowsNotFoundException()
        {
            // Arrange
            var createDto = new CreatePolicyDTO { SubtypeId = Guid.NewGuid() };
            _insuranceRepoMock.Setup(r => r.GetSubtypeByIdAsync(createDto.SubtypeId)).ReturnsAsync((InsuranceSubtype)null!);

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() => _policyService.CalculateQuoteAsync(createDto));
        }

        [Fact]
        public async Task ActivatePolicy_PolicyExists_ActivatesAndPublishesEvent()
        {
            // Arrange
            var policyId = Guid.NewGuid();
            var policy = new Policy { PolicyId = policyId, UserId = Guid.NewGuid(), Status = "Pending", SubtypeId = Guid.NewGuid() };
            _policyRepoMock.Setup(r => r.GetByIdAsync(policyId)).ReturnsAsync(policy);

            // Act
            await _policyService.ActivatePolicyAsync(policyId);

            // Assert
            Assert.Equal("Active", policy.Status);
            _policyRepoMock.Verify(r => r.UpdateAsync(policy), Times.Once);
            _policyRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
            _busMock.Verify(b => b.Publish(It.IsAny<SmartSure.Shared.Contracts.Events.PolicyActivatedEvent>(), default), Times.Once);
        }
    }
}
