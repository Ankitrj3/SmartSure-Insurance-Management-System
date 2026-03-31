using Microsoft.EntityFrameworkCore;
using SmartSure.PolicyService.Data;
using SmartSure.PolicyService.Models;

namespace SmartSure.PolicyService.Repositories
{
    /// <summary>
    /// Represent or implements DiscountRepository.
    /// </summary>
    public class DiscountRepository : IDiscountRepository
    {
        private readonly PolicyDbContext _context;

        public DiscountRepository(PolicyDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Performs the GetAllAsync operation.
        /// </summary>
        public async Task<List<Discount>> GetAllAsync()
        {
            return await _context.Discounts.ToListAsync();
        }

        /// <summary>
        /// Performs the GetByIdAsync operation.
        /// </summary>
        public async Task<Discount> GetByIdAsync(Guid discountId)
        {
            return await _context.Discounts.FindAsync(discountId);
        }

        /// <summary>
        /// Performs the GetByCodeAsync operation.
        /// </summary>
        public async Task<Discount> GetByCodeAsync(string code)
        {
            return await _context.Discounts
                .FirstOrDefaultAsync(d => d.Code.ToUpper() == code.ToUpper() && d.IsActive);
        }

        /// <summary>
        /// Performs the AddAsync operation.
        /// </summary>
        public async Task AddAsync(Discount discount)
        {
            await _context.Discounts.AddAsync(discount);
        }

        /// <summary>
        /// Performs the UpdateAsync operation.
        /// </summary>
        public async Task UpdateAsync(Discount discount)
        {
            _context.Discounts.Update(discount);
        }

        /// <summary>
        /// Performs the DeleteAsync operation.
        /// </summary>
        public async Task DeleteAsync(Guid discountId)
        {
            var discount = await _context.Discounts.FindAsync(discountId);
            if (discount != null) _context.Discounts.Remove(discount);
        }

        /// <summary>
        /// Performs the SaveChangesAsync operation.
        /// </summary>
        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
