using Microsoft.EntityFrameworkCore;
using SmartSure.ClaimsService.Data;
using SmartSure.ClaimsService.Models;

namespace SmartSure.ClaimsService.Repositories
{
    /// <summary>
    /// Represent or implements ClaimStatusHistoryRepository.
    /// </summary>
    public class ClaimStatusHistoryRepository : IClaimStatusHistoryRepository
    {
        private readonly ClaimsDbContext _context;

        public ClaimStatusHistoryRepository(ClaimsDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Performs the GetByClaimIdAsync operation.
        /// </summary>
        public async Task<List<ClaimStatusHistory>> GetByClaimIdAsync(Guid claimId)
        {
            return await _context.ClaimStatusHistory
                .Where(h => h.ClaimId == claimId)
                .OrderByDescending(h => h.ChangedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Performs the AddAsync operation.
        /// </summary>
        public async Task AddAsync(ClaimStatusHistory history)
        {
            await _context.ClaimStatusHistory.AddAsync(history);
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
