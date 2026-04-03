using SmartSure.PolicyService.DTOs;
using SmartSure.PolicyService.Models;
using SmartSure.PolicyService.Repositories;
using SmartSure.Shared.Contracts.Exceptions;

namespace SmartSure.PolicyService.Services
{
    /// <summary>
    /// Represent or implements DiscountService.
    /// </summary>
    public class DiscountService : IDiscountService
    {
        private readonly IDiscountRepository _repo;
        private readonly IPolicyRepository _policyRepo;

        public DiscountService(IDiscountRepository repo, IPolicyRepository policyRepo)
        {
            _repo       = repo;
            _policyRepo = policyRepo;
        }

        /// <summary>
        /// Performs the GetAllDiscountsAsync operation.
        /// </summary>
        public async Task<List<DiscountDTO>> GetAllDiscountsAsync()
        {
            var discounts = await _repo.GetAllAsync();
            return discounts.Select(MapToDto).ToList();
        }

        /// <summary>
        /// Performs the GetDiscountByIdAsync operation.
        /// </summary>
        public async Task<DiscountDTO> GetDiscountByIdAsync(Guid discountId)
        {
            var d = await _repo.GetByIdAsync(discountId);
            if (d == null) return null!;
            return MapToDto(d);
        }

        /// <summary>
        /// Performs the CreateDiscountAsync operation.
        /// </summary>
        public async Task<DiscountDTO> CreateDiscountAsync(CreateDiscountDTO dto)
        {
            var discount = new Discount
            {
                DiscountId       = Guid.NewGuid(),
                Code             = dto.Code.ToUpper(),
                Description      = dto.Description,
                Percentage       = dto.Percentage,
                MaxDiscountAmount = dto.MaxDiscountAmount,
                IsFirstTimeOnly  = dto.IsFirstTimeOnly,
                IsActive         = true,
                ValidFrom        = DateTime.UtcNow,
                ValidUntil       = dto.ValidUntil
            };

            await _repo.AddAsync(discount);
            await _repo.SaveChangesAsync();
            return MapToDto(discount);
        }

        /// <summary>
        /// Performs the UpdateDiscountAsync operation.
        /// </summary>
        public async Task UpdateDiscountAsync(Guid discountId, CreateDiscountDTO dto)
        {
            var discount = await _repo.GetByIdAsync(discountId);
            if (discount == null) throw new NotFoundException("Discount", discountId);

            discount.Code             = dto.Code.ToUpper();
            discount.Description      = dto.Description;
            discount.Percentage       = dto.Percentage;
            discount.MaxDiscountAmount = dto.MaxDiscountAmount;
            discount.IsFirstTimeOnly  = dto.IsFirstTimeOnly;
            discount.ValidUntil       = dto.ValidUntil;

            await _repo.UpdateAsync(discount);
            await _repo.SaveChangesAsync();
        }

        /// <summary>
        /// Performs the DeleteDiscountAsync operation.
        /// </summary>
        public async Task DeleteDiscountAsync(Guid discountId)
        {
            await _repo.DeleteAsync(discountId);
            await _repo.SaveChangesAsync();
        }

        /// <summary>
        /// Performs the CalculateDiscountAsync operation.
        /// </summary>
        public async Task<ApplyDiscountResultDTO> CalculateDiscountAsync(Guid userId, decimal originalPremium, string? couponCode)
        {
            decimal totalDiscountPercent = 0;
            string description           = "";
            bool firstTimeApplied        = false;
            string appliedCoupon         = "";

            // 1. First-time buyer → auto 10% discount
            var userPolicies = await _policyRepo.GetByUserIdAsync(userId, 1, 1);
            bool isFirstTime = userPolicies == null || userPolicies.TotalCount == 0;

            if (isFirstTime)
            {
                totalDiscountPercent += 10;
                description          += "🎉 First insurance purchase: 10% off! ";
                firstTimeApplied      = true;
            }

            if (!string.IsNullOrWhiteSpace(couponCode))
            {
                couponCode = couponCode.Trim().ToUpper();
                var coupon = await _repo.GetByCodeAsync(couponCode);
                if (coupon != null)
                {
                    bool isValid = coupon.IsActive
                        && (coupon.ValidUntil == null || coupon.ValidUntil >= DateTime.UtcNow);

                    if (isValid)
                    {
                        totalDiscountPercent += coupon.Percentage;
                        description          += $"Coupon {coupon.Code}: {coupon.Percentage}% off. ";
                        appliedCoupon         = coupon.Code;
                    }
                }
            }

            // Cap at max 30% total discount
            totalDiscountPercent = Math.Min(totalDiscountPercent, 30);
            decimal discountAmount = originalPremium * (totalDiscountPercent / 100);

            // Apply max cap from coupon if applicable
            if (!string.IsNullOrWhiteSpace(appliedCoupon))
            {
                var coupon = await _repo.GetByCodeAsync(appliedCoupon);
                if (coupon != null && coupon.MaxDiscountAmount > 0 && discountAmount > coupon.MaxDiscountAmount)
                {
                    discountAmount  = coupon.MaxDiscountAmount;
                    description    += $"(Capped at max ₹{coupon.MaxDiscountAmount}) ";
                }
            }

            decimal finalPremium = Math.Round(originalPremium - discountAmount, 2);

            return new ApplyDiscountResultDTO
            {
                OriginalPremium      = originalPremium,
                DiscountPercentage   = totalDiscountPercent,
                DiscountAmount       = Math.Round(discountAmount, 2),
                FinalPremium         = Math.Max(finalPremium, 0),
                DiscountDescription  = description.Trim(),
                FirstTimeDiscount    = firstTimeApplied,
                CouponCode           = appliedCoupon
            };
        }

        private static DiscountDTO MapToDto(Discount d)
        {
            return new DiscountDTO
            {
                DiscountId        = d.DiscountId,
                Code              = d.Code,
                Description       = d.Description,
                Percentage        = d.Percentage,
                MaxDiscountAmount = d.MaxDiscountAmount,
                IsFirstTimeOnly   = d.IsFirstTimeOnly,
                IsActive          = d.IsActive,
                ValidFrom         = d.ValidFrom,
                ValidUntil        = d.ValidUntil
            };
        }
    }
}
