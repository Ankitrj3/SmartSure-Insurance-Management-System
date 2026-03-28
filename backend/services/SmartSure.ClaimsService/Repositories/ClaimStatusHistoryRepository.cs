using Microsoft.EntityFrameworkCore;
using SmartSure.ClaimsService.Data;
using SmartSure.ClaimsService.Models;

namespace SmartSure.ClaimsService.Repositories
{
    public class ClaimStatusHistoryRepository : IClaimStatusHistoryRepository
    {
        private readonly ClaimsDbContext _context;

        public ClaimStatusHistoryRepository(ClaimsDbContext context)
        {
            _context = context;
        }

        public async Task<List<ClaimStatusHistory>> GetByClaimIdAsync(Guid claimId)
        {
            return await _context.ClaimStatusHistory
                .Where(h => h.ClaimId == claimId)
                .OrderByDescending(h => h.ChangedAt)
                .ToListAsync();
        }

        public async Task AddAsync(ClaimStatusHistory history)
        {
            await _context.ClaimStatusHistory.AddAsync(history);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
