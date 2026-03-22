using Microsoft.EntityFrameworkCore;
using SmartSure.PolicyService.Data;
using SmartSure.PolicyService.Models;

namespace SmartSure.PolicyService.Repositories
{
    public class DiscountRepository : IDiscountRepository
    {
        private readonly PolicyDbContext _context;

        public DiscountRepository(PolicyDbContext context)
        {
            _context = context;
        }

        public async Task<List<Discount>> GetAllAsync()
        {
            return await _context.Discounts.ToListAsync();
        }

        public async Task<Discount> GetByIdAsync(Guid discountId)
        {
            return await _context.Discounts.FindAsync(discountId);
        }

        public async Task<Discount> GetByCodeAsync(string code)
        {
            return await _context.Discounts
                .FirstOrDefaultAsync(d => d.Code.ToUpper() == code.ToUpper() && d.IsActive);
        }

        public async Task AddAsync(Discount discount)
        {
            await _context.Discounts.AddAsync(discount);
        }

        public async Task UpdateAsync(Discount discount)
        {
            _context.Discounts.Update(discount);
        }

        public async Task DeleteAsync(Guid discountId)
        {
            var discount = await _context.Discounts.FindAsync(discountId);
            if (discount != null) _context.Discounts.Remove(discount);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
