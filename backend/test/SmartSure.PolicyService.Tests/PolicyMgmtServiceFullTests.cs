using MassTransit;
using Microsoft.Extensions.Logging;
using Moq;
using SmartSure.PolicyService.DTOs;
using SmartSure.PolicyService.Models;
using SmartSure.PolicyService.Repositories;
using SmartSure.PolicyService.Services;
using SmartSure.Shared.Contracts.DTOs;
using SmartSure.Shared.Contracts.Exceptions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace SmartSure.PolicyService.Tests
{
    /// <summary>
    /// Comprehensive unit tests for PolicyMgmtService - covering IDV calculation (IRDAI schedule),
    /// premium computation with GST, policy lifecycle (Pending→Active→Cancelled→Terminated),
    /// and business-rule enforcement.
    /// </summary>
    public class PolicyMgmtServiceFullTests
    {
        private readonly Mock<IPolicyRepository>       _policyRepoMock;
        private readonly Mock<IInsuranceRepository>    _insuranceRepoMock;
        private readonly Mock<IBus>                    _busMock;
        private readonly Mock<IDiscountService>        _discountServiceMock;
        private readonly Mock<ILogger<PolicyMgmtService>> _loggerMock;
        private readonly PolicyMgmtService             _service;

        public PolicyMgmtServiceFullTests()
        {
            _policyRepoMock     = new Mock<IPolicyRepository>();
            _insuranceRepoMock  = new Mock<IInsuranceRepository>();
            _busMock            = new Mock<IBus>();
            _discountServiceMock= new Mock<IDiscountService>();
            _loggerMock         = new Mock<ILogger<PolicyMgmtService>>();

            _service = new PolicyMgmtService(
                _policyRepoMock.Object,
                _insuranceRepoMock.Object,
                _busMock.Object,
                _discountServiceMock.Object,
                _loggerMock.Object
            );
        }

        // ── Helpers ─────────────────────────────────────────────────────────────

        private (InsuranceSubtype subtype, InsuranceType type) CreateVehicleInsuranceType(decimal basePremium = 3000)
        {
            var typeId  = Guid.NewGuid();
            var type    = new InsuranceType { TypeId = typeId, Name = "Vehicle Insurance" };
            var subtype = new InsuranceSubtype { SubtypeId = Guid.NewGuid(), TypeId = typeId, Name = "Comprehensive", BasePremium = basePremium, Type = type };
            return (subtype, type);
        }

        private (InsuranceSubtype subtype, InsuranceType type) CreateHomeInsuranceType(decimal basePremium = 5000)
        {
            var typeId  = Guid.NewGuid();
            var type    = new InsuranceType { TypeId = typeId, Name = "Home Insurance" };
            var subtype = new InsuranceSubtype { SubtypeId = Guid.NewGuid(), TypeId = typeId, Name = "Standard Home", BasePremium = basePremium, Type = type };
            return (subtype, type);
        }

        private ApplyDiscountResultDTO NoDiscountResult(decimal premium) => new ApplyDiscountResultDTO
        {
            OriginalPremium    = premium,
            FinalPremium       = premium,
            DiscountPercentage = 0,
            DiscountAmount     = 0
        };

        // ── Quote: IDV Calculation Tests ──────────────────────────────────────

        [Theory]
        [InlineData(0,  0.05)]  // New car (≤ 6 months) → 5% depreciation
        [InlineData(1,  0.15)]  // 1 year old → 15%
        [InlineData(2,  0.20)]  // 2 years old → 20%
        [InlineData(3,  0.30)]  // 3 years old → 30%
        [InlineData(4,  0.40)]  // 4 years old → 40%
        [InlineData(5,  0.50)]  // 5 years old → 50%
        [InlineData(7,  0.60)]  // >5 years old → 60%
        public async Task CalculateQuote_Vehicle_AppliesCorrectIRDAIDepreciation(int ageInYears, double depreciationRate)
        {
            // Arrange
            var (subtype, type) = CreateVehicleInsuranceType(basePremium: 0); // 0 base for IDV-only test
            decimal exShowroom  = 600000m;
            int manufactureYear = DateTime.UtcNow.Year - ageInYears;

            _insuranceRepoMock.Setup(r => r.GetSubtypeByIdAsync(subtype.SubtypeId)).ReturnsAsync(subtype);
            _insuranceRepoMock.Setup(r => r.GetTypeByIdAsync(type.TypeId)).ReturnsAsync(type);

            var dto = new CreatePolicyDTO
            {
                SubtypeId     = subtype.SubtypeId,
                Duration      = 12,
                VehicleDetail = new PolicyVehicleDetailDTO
                {
                    RegistrationNumber = "TN01AB1234",
                    Make               = "Toyota",
                    Model              = "Camry",
                    ManufactureYear    = manufactureYear,
                    EstimatedValue     = exShowroom,
                    ChassisNumber      = "CHS123",
                    EngineNumber       = "ENG456"
                }
            };

            // Act
            var result = await _service.CalculateQuoteAsync(dto);

            // Assert
            decimal expectedIdv = Math.Max(exShowroom * (1 - (decimal)depreciationRate), 10000);
            Assert.Equal(expectedIdv, result.InsuredDeclaredValue);
        }

        [Fact]
        public async Task CalculateQuote_Vehicle_MinimumIdvIs10000()
        {
            // Arrange – very old, very cheap car
            var (subtype, type) = CreateVehicleInsuranceType(basePremium: 0);
            _insuranceRepoMock.Setup(r => r.GetSubtypeByIdAsync(subtype.SubtypeId)).ReturnsAsync(subtype);
            _insuranceRepoMock.Setup(r => r.GetTypeByIdAsync(type.TypeId)).ReturnsAsync(type);

            var dto = new CreatePolicyDTO
            {
                SubtypeId     = subtype.SubtypeId,
                Duration      = 12,
                VehicleDetail = new PolicyVehicleDetailDTO
                {
                    EstimatedValue  = 1000m,   // Only ₹1000; after 60% depreciation → ₹400, but floor is ₹10,000
                    ManufactureYear = 2010,
                    Make = "Old", Model = "Car", RegistrationNumber = "XX", ChassisNumber = "C", EngineNumber = "E"
                }
            };

            // Act
            var result = await _service.CalculateQuoteAsync(dto);

            // Assert
            Assert.Equal(10000m, result.InsuredDeclaredValue);
        }

        [Fact]
        public async Task CalculateQuote_Home_IdvIsAreaTimesReconstructionCost()
        {
            // Arrange
            var (subtype, type) = CreateHomeInsuranceType(basePremium: 0);
            _insuranceRepoMock.Setup(r => r.GetSubtypeByIdAsync(subtype.SubtypeId)).ReturnsAsync(subtype);
            _insuranceRepoMock.Setup(r => r.GetTypeByIdAsync(type.TypeId)).ReturnsAsync(type);

            var dto = new CreatePolicyDTO
            {
                SubtypeId  = subtype.SubtypeId,
                Duration   = 12,
                HomeDetail = new PolicyHomeDetailDTO
                {
                    Address                 = "123 Main St",
                    PropertyType            = "Apartment",
                    AreaSqFt                = 1000m,
                    ConstructionCostPerSqFt = 3000m   // IDV = 1000 * 3000 = 30,00,000
                }
            };

            // Act
            var result = await _service.CalculateQuoteAsync(dto);

            // Assert
            Assert.Equal(3_000_000m, result.InsuredDeclaredValue);
        }

        [Fact]
        public async Task CalculateQuote_Home_MinimumIdvIs50000()
        {
            // Arrange
            var (subtype, type) = CreateHomeInsuranceType(basePremium: 0);
            _insuranceRepoMock.Setup(r => r.GetSubtypeByIdAsync(subtype.SubtypeId)).ReturnsAsync(subtype);
            _insuranceRepoMock.Setup(r => r.GetTypeByIdAsync(type.TypeId)).ReturnsAsync(type);

            var dto = new CreatePolicyDTO
            {
                SubtypeId  = subtype.SubtypeId,
                Duration   = 12,
                HomeDetail = new PolicyHomeDetailDTO
                {
                    Address                 = "Tiny House",
                    PropertyType            = "Shed",
                    AreaSqFt                = 10m,
                    ConstructionCostPerSqFt = 100m  // IDV = 10 * 100 = 1000, below 50000 minimum
                }
            };

            // Act
            var result = await _service.CalculateQuoteAsync(dto);

            // Assert
            Assert.Equal(50000m, result.InsuredDeclaredValue);
        }

        // ── Quote: Premium Calculation with GST ──────────────────────────────

        [Fact]
        public async Task CalculateQuote_Premium_Includes18PercentGST()
        {
            // Arrange
            var (subtype, type) = CreateVehicleInsuranceType(basePremium: 10000m);
            _insuranceRepoMock.Setup(r => r.GetSubtypeByIdAsync(subtype.SubtypeId)).ReturnsAsync(subtype);
            _insuranceRepoMock.Setup(r => r.GetTypeByIdAsync(type.TypeId)).ReturnsAsync(type);

            var dto = new CreatePolicyDTO
            {
                SubtypeId     = subtype.SubtypeId,
                Duration      = 12,              // 1 year
                VehicleDetail = new PolicyVehicleDetailDTO
                {
                    EstimatedValue  = 500000m,
                    ManufactureYear = DateTime.UtcNow.Year,
                    Make = "Honda", Model = "City", RegistrationNumber = "MH01", ChassisNumber = "C", EngineNumber = "E"
                }
            };

            // Act
            var result = await _service.CalculateQuoteAsync(dto);

            // Assert: 
            // IDV = 500000 * 0.95 = 475000. 
            // Base Amount = 10000 * 1 + 475000 * 0.025 = 10000 + 11875 = 21875.
            // With GST (18%) = 21875 * 1.18 = 25812.50
            Assert.Equal(25812.50m, result.PremiumAmount);
        }

        [Fact]
        public async Task CalculateQuote_SubtypeNotFound_ThrowsNotFoundException()
        {
            // Arrange
            var dto = new CreatePolicyDTO { SubtypeId = Guid.NewGuid(), Duration = 12 };
            _insuranceRepoMock.Setup(r => r.GetSubtypeByIdAsync(dto.SubtypeId)).ReturnsAsync((InsuranceSubtype)null!);

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() => _service.CalculateQuoteAsync(dto));
        }

        // ── CreatePolicy Tests ────────────────────────────────────────────────

        [Fact]
        public async Task CreatePolicy_Vehicle_StoresPolicyAndVehicleDetail()
        {
            // Arrange
            var (subtype, type) = CreateVehicleInsuranceType();
            var userId = Guid.NewGuid();

            _insuranceRepoMock.Setup(r => r.GetSubtypeByIdAsync(subtype.SubtypeId)).ReturnsAsync(subtype);
            _insuranceRepoMock.Setup(r => r.GetTypeByIdAsync(type.TypeId)).ReturnsAsync(type);
            _discountServiceMock.Setup(d => d.CalculateDiscountAsync(userId, It.IsAny<decimal>(), null))
                .ReturnsAsync((Guid _, decimal p, string? __) => NoDiscountResult(p));
            _policyRepoMock.Setup(r => r.AddAsync(It.IsAny<Policy>())).Returns(Task.CompletedTask);
            _policyRepoMock.Setup(r => r.AddOrUpdateVehicleDetailAsync(It.IsAny<VehicleDetail>())).Returns(Task.CompletedTask);
            _policyRepoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

            var dto = new CreatePolicyDTO
            {
                SubtypeId     = subtype.SubtypeId,
                Duration      = 12,
                NomineeName   = "Jane Doe",
                NomineeRelation = "Spouse",
                VehicleDetail = new PolicyVehicleDetailDTO
                {
                    RegistrationNumber = "KA05HH9999",
                    Make               = "Maruti",
                    Model              = "Swift",
                    ManufactureYear    = DateTime.UtcNow.Year - 1,
                    EstimatedValue     = 600000m,
                    ChassisNumber      = "CHAS001",
                    EngineNumber       = "ENG001"
                }
            };

            // Act
            var result = await _service.CreatePolicyAsync(userId, dto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Pending",    result.Status);
            Assert.Equal(userId,       result.UserId);
            Assert.Equal("Jane Doe",   result.NomineeName);
            Assert.Equal("Spouse",     result.NomineeRelation);
            _policyRepoMock.Verify(r => r.AddAsync(It.IsAny<Policy>()), Times.Once);
            _policyRepoMock.Verify(r => r.AddOrUpdateVehicleDetailAsync(It.IsAny<VehicleDetail>()), Times.Once);
            _policyRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task CreatePolicy_Home_StoresPolicyAndHomeDetail()
        {
            // Arrange
            var (subtype, type) = CreateHomeInsuranceType();
            var userId = Guid.NewGuid();

            _insuranceRepoMock.Setup(r => r.GetSubtypeByIdAsync(subtype.SubtypeId)).ReturnsAsync(subtype);
            _insuranceRepoMock.Setup(r => r.GetTypeByIdAsync(type.TypeId)).ReturnsAsync(type);
            _discountServiceMock.Setup(d => d.CalculateDiscountAsync(userId, It.IsAny<decimal>(), null))
                .ReturnsAsync((Guid _, decimal p, string? __) => NoDiscountResult(p));
            _policyRepoMock.Setup(r => r.AddAsync(It.IsAny<Policy>())).Returns(Task.CompletedTask);
            _policyRepoMock.Setup(r => r.AddOrUpdateHomeDetailAsync(It.IsAny<HomeDetail>())).Returns(Task.CompletedTask);
            _policyRepoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

            var dto = new CreatePolicyDTO
            {
                SubtypeId  = subtype.SubtypeId,
                Duration   = 12,
                HomeDetail = new PolicyHomeDetailDTO
                {
                    Address                 = "456 Park Ave",
                    PropertyType            = "Independent House",
                    YearBuilt               = 2010,
                    AreaSqFt                = 1500m,
                    ConstructionCostPerSqFt = 2500m,
                    SecurityFeatures        = "CCTV, Guard"
                }
            };

            // Act
            var result = await _service.CreatePolicyAsync(userId, dto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Pending", result.Status);
            _policyRepoMock.Verify(r => r.AddOrUpdateHomeDetailAsync(It.IsAny<HomeDetail>()), Times.Once);
        }

        [Fact]
        public async Task CreatePolicy_WithCouponCode_AppliesDiscount()
        {
            // Arrange
            var (subtype, type) = CreateVehicleInsuranceType(basePremium: 10000m);
            var userId = Guid.NewGuid();

            _insuranceRepoMock.Setup(r => r.GetSubtypeByIdAsync(subtype.SubtypeId)).ReturnsAsync(subtype);
            _insuranceRepoMock.Setup(r => r.GetTypeByIdAsync(type.TypeId)).ReturnsAsync(type);

            decimal basePremium      = 11800m; // After 18% GST on 10000
            decimal discountedAmount =  9440m; // 20% off

            _discountServiceMock.Setup(d => d.CalculateDiscountAsync(userId, It.IsAny<decimal>(), "SAVE20"))
                .ReturnsAsync(new ApplyDiscountResultDTO
                {
                    OriginalPremium    = basePremium,
                    DiscountPercentage = 20,
                    DiscountAmount     = basePremium * 0.20m,
                    FinalPremium       = discountedAmount
                });

            _policyRepoMock.Setup(r => r.AddAsync(It.IsAny<Policy>())).Returns(Task.CompletedTask);
            _policyRepoMock.Setup(r => r.AddOrUpdateVehicleDetailAsync(It.IsAny<VehicleDetail>())).Returns(Task.CompletedTask);
            _policyRepoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

            var dto = new CreatePolicyDTO
            {
                SubtypeId   = subtype.SubtypeId,
                Duration    = 12,
                CouponCode  = "SAVE20",
                VehicleDetail = new PolicyVehicleDetailDTO
                {
                    EstimatedValue  = 500000m,
                    ManufactureYear = DateTime.UtcNow.Year,
                    Make = "KIA", Model = "Seltos", RegistrationNumber = "DL01", ChassisNumber = "C", EngineNumber = "E"
                }
            };

            // Act
            var result = await _service.CreatePolicyAsync(userId, dto);

            // Assert
            Assert.Equal(9440m, result.PremiumAmount);
        }

        [Fact]
        public async Task CreatePolicy_SubtypeNotFound_ThrowsNotFoundException()
        {
            // Arrange
            _insuranceRepoMock.Setup(r => r.GetSubtypeByIdAsync(It.IsAny<Guid>())).ReturnsAsync((InsuranceSubtype)null!);
            var dto = new CreatePolicyDTO { SubtypeId = Guid.NewGuid(), Duration = 12 };

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() => _service.CreatePolicyAsync(Guid.NewGuid(), dto));
        }

        // ── ActivatePolicy Tests ──────────────────────────────────────────────

        [Fact]
        public async Task ActivatePolicy_PendingPolicy_SetsStatusToActiveAndPublishesEvent()
        {
            // Arrange
            var policyId = Guid.NewGuid();
            var policy   = new Policy { PolicyId = policyId, UserId = Guid.NewGuid(), Status = "Pending", SubtypeId = Guid.NewGuid() };

            _policyRepoMock.Setup(r => r.GetByIdAsync(policyId)).ReturnsAsync(policy);
            _policyRepoMock.Setup(r => r.UpdateAsync(policy)).Returns(Task.CompletedTask);
            _policyRepoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

            // Act
            await _service.ActivatePolicyAsync(policyId);

            // Assert
            Assert.Equal("Active", policy.Status);
            _policyRepoMock.Verify(r => r.UpdateAsync(policy), Times.Once);
            _policyRepoMock.Verify(r => r.SaveChangesAsync(),  Times.Once);
            _busMock.Verify(b => b.Publish(It.IsAny<SmartSure.Shared.Contracts.Events.PolicyActivatedEvent>(), default), Times.Once);
        }

        [Fact]
        public async Task ActivatePolicy_AlreadyActivePolicy_DoesNotCallUpdateOrPublish()
        {
            // Arrange
            var policyId = Guid.NewGuid();
            var policy   = new Policy { PolicyId = policyId, Status = "Active", SubtypeId = Guid.NewGuid() };

            _policyRepoMock.Setup(r => r.GetByIdAsync(policyId)).ReturnsAsync(policy);

            // Act
            await _service.ActivatePolicyAsync(policyId);

            // Assert – idempotent; no update should happen
            _policyRepoMock.Verify(r => r.UpdateAsync(It.IsAny<Policy>()), Times.Never);
            _busMock.Verify(b => b.Publish(It.IsAny<SmartSure.Shared.Contracts.Events.PolicyActivatedEvent>(), default), Times.Never);
        }

        [Fact]
        public async Task ActivatePolicy_PolicyNotFound_ThrowsNotFoundException()
        {
            // Arrange
            var policyId = Guid.NewGuid();
            _policyRepoMock.Setup(r => r.GetByIdAsync(policyId)).ReturnsAsync((Policy)null!);

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() => _service.ActivatePolicyAsync(policyId));
        }

        // ── CancelPolicy Tests ────────────────────────────────────────────────

        [Fact]
        public async Task CancelPolicy_PolicyExists_CallsCancelAndPublishesEvent()
        {
            // Arrange
            var policyId = Guid.NewGuid();
            var policy   = new Policy { PolicyId = policyId, UserId = Guid.NewGuid(), Status = "Active" };

            _policyRepoMock.Setup(r => r.GetByIdAsync(policyId)).ReturnsAsync(policy);
            _policyRepoMock.Setup(r => r.CancelAsync(policyId)).Returns(Task.CompletedTask);

            // Act
            await _service.CancelPolicyAsync(policyId);

            // Assert
            _policyRepoMock.Verify(r => r.CancelAsync(policyId), Times.Once);
            _busMock.Verify(b => b.Publish(It.IsAny<SmartSure.Shared.Contracts.Events.PolicyCancelledEvent>(), default), Times.Once);
        }

        [Fact]
        public async Task CancelPolicy_PolicyNotFound_DoesNotThrow()
        {
            // Arrange
            var policyId = Guid.NewGuid();
            _policyRepoMock.Setup(r => r.GetByIdAsync(policyId)).ReturnsAsync((Policy)null!);

            // Act – should not throw; no-op when policy not found
            await _service.CancelPolicyAsync(policyId);

            // Assert
            _policyRepoMock.Verify(r => r.CancelAsync(It.IsAny<Guid>()), Times.Never);
        }

        // ── FailPolicy Tests ──────────────────────────────────────────────────

        [Fact]
        public async Task FailPolicy_PolicyExists_CreatesFailedPaymentAndSetsStatusToFailed()
        {
            // Arrange
            var policyId = Guid.NewGuid();
            var policy   = new Policy { PolicyId = policyId, PremiumAmount = 15000m, Status = "Pending" };

            _policyRepoMock.Setup(r => r.GetByIdAsync(policyId)).ReturnsAsync(policy);
            _policyRepoMock.Setup(r => r.AddPaymentAsync(It.IsAny<Payment>())).Returns(Task.CompletedTask);
            _policyRepoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

            // Act
            await _service.FailPolicyAsync(policyId);

            // Assert
            Assert.Equal("Failed", policy.Status);
            _policyRepoMock.Verify(r => r.AddPaymentAsync(It.Is<Payment>(p =>
                p.Status  == "Failed" &&
                p.Amount  == 15000m  &&
                p.PolicyId == policyId)), Times.Once);
            _policyRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        // ── TerminatePolicy Tests ─────────────────────────────────────────────

        [Fact]
        public async Task TerminatePolicy_PolicyExists_SetsIsTerminatedAndStatusCancelled()
        {
            // Arrange
            var policyId = Guid.NewGuid();
            var policy   = new Policy { PolicyId = policyId, Status = "Active", IsTerminated = false };

            _policyRepoMock.Setup(r => r.GetByIdAsync(policyId)).ReturnsAsync(policy);
            _policyRepoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

            // Act
            await _service.TerminatePolicyAsync(policyId);

            // Assert
            Assert.True(policy.IsTerminated);
            Assert.Equal("Cancelled", policy.Status);
            _policyRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task TerminatePolicy_PolicyNotFound_ThrowsNotFoundException()
        {
            // Arrange
            var policyId = Guid.NewGuid();
            _policyRepoMock.Setup(r => r.GetByIdAsync(policyId)).ReturnsAsync((Policy)null!);

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() => _service.TerminatePolicyAsync(policyId));
        }

        // ── IncrementApprovedClaimsCount Tests ───────────────────────────────

        [Fact]
        public async Task IncrementApprovedClaimsCount_PolicyExists_IncrementsCounter()
        {
            // Arrange
            var policyId = Guid.NewGuid();
            var policy   = new Policy { PolicyId = policyId, ApprovedClaimsCount = 1 };

            _policyRepoMock.Setup(r => r.GetByIdAsync(policyId)).ReturnsAsync(policy);
            _policyRepoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

            // Act
            await _service.IncrementApprovedClaimsCountAsync(policyId);

            // Assert
            Assert.Equal(2, policy.ApprovedClaimsCount);
            _policyRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task IncrementApprovedClaimsCount_PolicyNotFound_ThrowsNotFoundException()
        {
            // Arrange
            var policyId = Guid.NewGuid();
            _policyRepoMock.Setup(r => r.GetByIdAsync(policyId)).ReturnsAsync((Policy)null!);

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() => _service.IncrementApprovedClaimsCountAsync(policyId));
        }

        // ── GetPremiumAmount Tests ─────────────────────────────────────────────

        [Fact]
        public async Task GetPremiumAmount_PolicyExists_ReturnsPremiumAmount()
        {
            // Arrange
            var policyId = Guid.NewGuid();
            var policy   = new Policy { PolicyId = policyId, PremiumAmount = 12350m };
            _policyRepoMock.Setup(r => r.GetByIdAsync(policyId)).ReturnsAsync(policy);

            // Act
            var result = await _service.GetPremiumAmountAsync(policyId);

            // Assert
            Assert.Equal(12350m, result);
        }

        [Fact]
        public async Task GetPremiumAmount_PolicyNotFound_ThrowsNotFoundException()
        {
            // Arrange
            var policyId = Guid.NewGuid();
            _policyRepoMock.Setup(r => r.GetByIdAsync(policyId)).ReturnsAsync((Policy)null!);

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() => _service.GetPremiumAmountAsync(policyId));
        }

        // ── DeletePolicy Tests ────────────────────────────────────────────────

        [Fact]
        public async Task DeletePolicy_PolicyNotFound_DoesNotThrow()
        {
            // Arrange
            var policyId = Guid.NewGuid();
            _policyRepoMock.Setup(r => r.GetByIdAsync(policyId)).ReturnsAsync((Policy)null!);

            // Act – no-op when policy not found
            await _service.DeletePolicyAsync(policyId);

            // Assert
            _policyRepoMock.Verify(r => r.DeleteAsync(It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task DeletePolicy_PolicyExists_CallsDeleteAsync()
        {
            // Arrange
            var policyId = Guid.NewGuid();
            var policy   = new Policy { PolicyId = policyId };
            _policyRepoMock.Setup(r => r.GetByIdAsync(policyId)).ReturnsAsync(policy);
            _policyRepoMock.Setup(r => r.DeleteAsync(policyId)).Returns(Task.CompletedTask);

            // Act
            await _service.DeletePolicyAsync(policyId);

            // Assert
            _policyRepoMock.Verify(r => r.DeleteAsync(policyId), Times.Once);
        }
    }
}
