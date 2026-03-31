using SmartSure.PolicyService.Models;

namespace SmartSure.PolicyService.Repositories
{
    /// <summary>
    /// Represent or implements IDiscountRepository.
    /// </summary>
    public interface IDiscountRepository
    {
        Task<List<Discount>> GetAllAsync();
        Task<Discount> GetByIdAsync(Guid discountId);
        Task<Discount> GetByCodeAsync(string code);
        Task AddAsync(Discount discount);
        Task UpdateAsync(Discount discount);
        Task DeleteAsync(Guid discountId);
        Task SaveChangesAsync();
    }
}
