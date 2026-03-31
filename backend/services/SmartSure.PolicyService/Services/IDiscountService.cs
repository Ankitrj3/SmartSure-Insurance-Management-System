using SmartSure.PolicyService.DTOs;

namespace SmartSure.PolicyService.Services
{
    /// <summary>
    /// Represent or implements IDiscountService.
    /// </summary>
    public interface IDiscountService
    {
        Task<List<DiscountDTO>> GetAllDiscountsAsync();
        Task<DiscountDTO> GetDiscountByIdAsync(Guid discountId);
        Task<DiscountDTO> CreateDiscountAsync(CreateDiscountDTO dto);
        Task UpdateDiscountAsync(Guid discountId, CreateDiscountDTO dto);
        Task DeleteDiscountAsync(Guid discountId);

        /// <summary>
        /// Calculate the discount for a premium. Checks coupon code validity and first-time purchase.
        /// </summary>
        Task<ApplyDiscountResultDTO> CalculateDiscountAsync(Guid userId, decimal originalPremium, string? couponCode);
    }
}
