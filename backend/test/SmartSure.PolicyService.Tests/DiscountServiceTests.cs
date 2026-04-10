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
    /// Unit tests for DiscountService - covering discount CRUD, coupon validation,
    /// first-time buyer detection, discount stacking, and cap enforcement.
    /// </summary>
    public class DiscountServiceTests
    {
        private readonly Mock<IDiscountRepository> _discountRepoMock;
        private readonly Mock<IPolicyRepository>   _policyRepoMock;
        private readonly DiscountService           _service;

        public DiscountServiceTests()
        {
            _discountRepoMock = new Mock<IDiscountRepository>();
            _policyRepoMock   = new Mock<IPolicyRepository>();
            _service          = new DiscountService(_discountRepoMock.Object, _policyRepoMock.Object);
        }

        // ── Helper ─────────────────────────────────────────────────────────────

        private static Discount MakeDiscount(string code, decimal percent, decimal maxCap = 0, bool isActive = true, DateTime? validUntil = null)
        {
            return new Discount
            {
                DiscountId        = Guid.NewGuid(),
                Code              = code,
                Description       = $"{percent}% off",
                Percentage        = percent,
                MaxDiscountAmount = maxCap,
                IsActive          = isActive,
                ValidFrom         = DateTime.UtcNow.AddDays(-10),
                ValidUntil        = validUntil
            };
        }

        private static PagedResult<Policy> EmptyPagedResult() => new PagedResult<Policy>
        {
            Page       = 1,
            PageSize   = 1,
            TotalCount = 0,
            Items      = new List<Policy>()
        };

        private static PagedResult<Policy> NonEmptyPagedResult() => new PagedResult<Policy>
        {
            Page       = 1,
            PageSize   = 1,
            TotalCount = 2,
            Items      = new List<Policy> { new Policy(), new Policy() }
        };

        // ── Calculate Discount Tests ───────────────────────────────────────────

        [Fact]
        public async Task CalculateDiscount_FirstTimeBuyer_NoCode_Gets10PercentOff()
        {
            // Arrange
            var userId = Guid.NewGuid();
            _policyRepoMock.Setup(r => r.GetByUserIdAsync(userId, 1, 1)).ReturnsAsync(EmptyPagedResult());

            // Act
            var result = await _service.CalculateDiscountAsync(userId, 10000m, null);

            // Assert
            Assert.Equal(10m,    result.DiscountPercentage);
            Assert.Equal(1000m,  result.DiscountAmount);
            Assert.Equal(9000m,  result.FinalPremium);
            Assert.True(result.FirstTimeDiscount);
        }

        [Fact]
        public async Task CalculateDiscount_ReturningBuyer_NoCode_NoDiscount()
        {
            // Arrange
            var userId = Guid.NewGuid();
            _policyRepoMock.Setup(r => r.GetByUserIdAsync(userId, 1, 1)).ReturnsAsync(NonEmptyPagedResult());

            // Act
            var result = await _service.CalculateDiscountAsync(userId, 10000m, null);

            // Assert
            Assert.Equal(0m,     result.DiscountPercentage);
            Assert.Equal(0m,     result.DiscountAmount);
            Assert.Equal(10000m, result.FinalPremium);
            Assert.False(result.FirstTimeDiscount);
        }

        [Fact]
        public async Task CalculateDiscount_ValidCoupon_AppliesCouponDiscount()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var coupon = MakeDiscount("SAVE15", 15m);

            _policyRepoMock.Setup(r => r.GetByUserIdAsync(userId, 1, 1)).ReturnsAsync(NonEmptyPagedResult());
            _discountRepoMock.Setup(r => r.GetByCodeAsync("SAVE15")).ReturnsAsync(coupon);

            // Act
            var result = await _service.CalculateDiscountAsync(userId, 10000m, "SAVE15");

            // Assert
            Assert.Equal(15m,    result.DiscountPercentage);
            Assert.Equal(1500m,  result.DiscountAmount);
            Assert.Equal(8500m,  result.FinalPremium);
            Assert.Equal("SAVE15", result.CouponCode);
        }

        [Fact]
        public async Task CalculateDiscount_FirstTimeBuyerPlusCoupon_StacksDiscounts()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var coupon = MakeDiscount("FIRST15", 15m);

            _policyRepoMock.Setup(r => r.GetByUserIdAsync(userId, 1, 1)).ReturnsAsync(EmptyPagedResult());
            _discountRepoMock.Setup(r => r.GetByCodeAsync("FIRST15")).ReturnsAsync(coupon);

            // Act – first-time (10%) + coupon (15%) = 25% total
            var result = await _service.CalculateDiscountAsync(userId, 10000m, "FIRST15");

            // Assert
            Assert.Equal(25m,   result.DiscountPercentage);
            Assert.Equal(2500m, result.DiscountAmount);
            Assert.Equal(7500m, result.FinalPremium);
        }

        [Fact]
        public async Task CalculateDiscount_TotalDiscountCappedAt30Percent()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var coupon = MakeDiscount("BIGDEAL", 25m); // 10% first-time + 25% coupon = 35%, but capped at 30%

            _policyRepoMock.Setup(r => r.GetByUserIdAsync(userId, 1, 1)).ReturnsAsync(EmptyPagedResult());
            _discountRepoMock.Setup(r => r.GetByCodeAsync("BIGDEAL")).ReturnsAsync(coupon);

            // Act
            var result = await _service.CalculateDiscountAsync(userId, 10000m, "BIGDEAL");

            // Assert – capped at 30%
            Assert.Equal(30m,   result.DiscountPercentage);
            Assert.Equal(3000m, result.DiscountAmount);
            Assert.Equal(7000m, result.FinalPremium);
        }

        [Fact]
        public async Task CalculateDiscount_CouponWithMaxCap_CapsDollarAmount()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var coupon = MakeDiscount("CAPPED", 20m, maxCap: 500m); // 20% of 10000 = 2000, but max is ₹500

            _policyRepoMock.Setup(r => r.GetByUserIdAsync(userId, 1, 1)).ReturnsAsync(NonEmptyPagedResult());
            _discountRepoMock.Setup(r => r.GetByCodeAsync("CAPPED")).ReturnsAsync(coupon);
            // Called twice in service (once for validation, once for cap check)
            _discountRepoMock.Setup(r => r.GetByCodeAsync("CAPPED")).ReturnsAsync(coupon);

            // Act
            var result = await _service.CalculateDiscountAsync(userId, 10000m, "CAPPED");

            // Assert
            Assert.Equal(500m,  result.DiscountAmount);
            Assert.Equal(9500m, result.FinalPremium);
        }

        [Fact]
        public async Task CalculateDiscount_ExpiredCoupon_NoDiscountApplied()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var coupon = MakeDiscount("EXPIRED", 15m, validUntil: DateTime.UtcNow.AddDays(-1)); // Expired yesterday

            _policyRepoMock.Setup(r => r.GetByUserIdAsync(userId, 1, 1)).ReturnsAsync(NonEmptyPagedResult());
            _discountRepoMock.Setup(r => r.GetByCodeAsync("EXPIRED")).ReturnsAsync(coupon);

            // Act
            var result = await _service.CalculateDiscountAsync(userId, 10000m, "EXPIRED");

            // Assert – expired coupon should not apply any discount
            Assert.Equal(0m,     result.DiscountPercentage);
            Assert.Equal(10000m, result.FinalPremium);
        }

        [Fact]
        public async Task CalculateDiscount_InactiveCoupon_NoDiscountApplied()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var coupon = MakeDiscount("INACTIVE", 15m, isActive: false);

            _policyRepoMock.Setup(r => r.GetByUserIdAsync(userId, 1, 1)).ReturnsAsync(NonEmptyPagedResult());
            _discountRepoMock.Setup(r => r.GetByCodeAsync("INACTIVE")).ReturnsAsync(coupon);

            // Act
            var result = await _service.CalculateDiscountAsync(userId, 10000m, "INACTIVE");

            // Assert
            Assert.Equal(0m,     result.DiscountPercentage);
            Assert.Equal(10000m, result.FinalPremium);
        }

        [Fact]
        public async Task CalculateDiscount_NonExistentCoupon_NoDiscountApplied()
        {
            // Arrange
            var userId = Guid.NewGuid();
            _policyRepoMock.Setup(r => r.GetByUserIdAsync(userId, 1, 1)).ReturnsAsync(NonEmptyPagedResult());
            _discountRepoMock.Setup(r => r.GetByCodeAsync("GHOST")).ReturnsAsync((Discount)null!);

            // Act
            var result = await _service.CalculateDiscountAsync(userId, 10000m, "GHOST");

            // Assert
            Assert.Equal(0m,     result.DiscountPercentage);
            Assert.Equal(10000m, result.FinalPremium);
        }

        [Fact]
        public async Task CalculateDiscount_FinalPremiumNeverGoesNegative()
        {
            // Arrange
            var userId = Guid.NewGuid();
            _policyRepoMock.Setup(r => r.GetByUserIdAsync(userId, 1, 1)).ReturnsAsync(EmptyPagedResult());

            // Act – even with 30% cap, final must be ≥ 0
            var result = await _service.CalculateDiscountAsync(userId, 0m, null);

            // Assert
            Assert.True(result.FinalPremium >= 0);
        }

        // ── Coupon CRUD Tests ─────────────────────────────────────────────────

        [Fact]
        public async Task CreateDiscount_StoresDiscountWithUppercaseCode()
        {
            // Arrange
            Discount? savedDiscount = null;
            _discountRepoMock.Setup(r => r.AddAsync(It.IsAny<Discount>()))
                .Callback<Discount>(d => savedDiscount = d)
                .Returns(Task.CompletedTask);
            _discountRepoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

            var dto = new CreateDiscountDTO
            {
                Code              = "newcoupon",    // lowercase input
                Description       = "Test discount",
                Percentage        = 10m,
                MaxDiscountAmount = 0,
                IsFirstTimeOnly   = false,
                ValidUntil        = DateTime.UtcNow.AddDays(30)
            };

            // Act
            var result = await _service.CreateDiscountAsync(dto);

            // Assert
            Assert.Equal("NEWCOUPON",    result.Code);       // Code should be uppercased
            Assert.Equal(10m,            result.Percentage);
            Assert.True(result.IsActive);                    // New discounts are active by default
            _discountRepoMock.Verify(r => r.AddAsync(It.IsAny<Discount>()), Times.Once);
            _discountRepoMock.Verify(r => r.SaveChangesAsync(),              Times.Once);
        }

        [Fact]
        public async Task UpdateDiscount_DiscountNotFound_ThrowsNotFoundException()
        {
            // Arrange
            var discountId = Guid.NewGuid();
            _discountRepoMock.Setup(r => r.GetByIdAsync(discountId)).ReturnsAsync((Discount)null!);

            var dto = new CreateDiscountDTO { Code = "X", Description = "Y", Percentage = 5m };

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() => _service.UpdateDiscountAsync(discountId, dto));
        }

        [Fact]
        public async Task UpdateDiscount_ExistingDiscount_UpdatesFieldsAndSaves()
        {
            // Arrange
            var discountId = Guid.NewGuid();
            var existing   = MakeDiscount("OLD", 5m);
            _discountRepoMock.Setup(r => r.GetByIdAsync(discountId)).ReturnsAsync(existing);
            _discountRepoMock.Setup(r => r.UpdateAsync(existing)).Returns(Task.CompletedTask);
            _discountRepoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

            var dto = new CreateDiscountDTO
            {
                Code        = "updated",
                Description = "Updated Desc",
                Percentage  = 25m
            };

            // Act
            await _service.UpdateDiscountAsync(discountId, dto);

            // Assert
            Assert.Equal("UPDATED",       existing.Code);
            Assert.Equal("Updated Desc",  existing.Description);
            Assert.Equal(25m,             existing.Percentage);
            _discountRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task DeleteDiscount_CallsDeleteAndSave()
        {
            // Arrange
            var discountId = Guid.NewGuid();
            _discountRepoMock.Setup(r => r.DeleteAsync(discountId)).Returns(Task.CompletedTask);
            _discountRepoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

            // Act
            await _service.DeleteDiscountAsync(discountId);

            // Assert
            _discountRepoMock.Verify(r => r.DeleteAsync(discountId),  Times.Once);
            _discountRepoMock.Verify(r => r.SaveChangesAsync(),         Times.Once);
        }

        [Fact]
        public async Task GetAllDiscounts_ReturnsMappedDTOs()
        {
            // Arrange
            var discounts = new List<Discount>
            {
                MakeDiscount("A", 5m),
                MakeDiscount("B", 10m)
            };
            _discountRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(discounts);

            // Act
            var result = await _service.GetAllDiscountsAsync();

            // Assert
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task GetDiscountById_NotFound_ReturnsNull()
        {
            // Arrange
            var id = Guid.NewGuid();
            _discountRepoMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((Discount)null!);

            // Act
            var result = await _service.GetDiscountByIdAsync(id);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetDiscountById_Found_ReturnsMappedDTO()
        {
            // Arrange
            var discount = MakeDiscount("TESTCODE", 20m);
            _discountRepoMock.Setup(r => r.GetByIdAsync(discount.DiscountId)).ReturnsAsync(discount);

            // Act
            var result = await _service.GetDiscountByIdAsync(discount.DiscountId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("TESTCODE", result.Code);
            Assert.Equal(20m,        result.Percentage);
        }
    }
}
