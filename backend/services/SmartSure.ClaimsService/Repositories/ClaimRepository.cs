using Microsoft.EntityFrameworkCore;
using SmartSure.ClaimsService.Data;
using SmartSure.ClaimsService.Models;

namespace SmartSure.ClaimsService.Repositories
{
    public class ClaimRepository : IClaimRepository
    {
        private readonly ClaimsDbContext _context;

        public ClaimRepository(ClaimsDbContext context)
        {
            _context = context;
        }

        public async Task<Claim> GetByIdAsync(Guid claimId)
        {
            return await _context.Claims
                .Include(c => c.Documents)
                .Include(c => c.StatusHistory)
                .FirstOrDefaultAsync(c => c.ClaimId == claimId);
        }

        public async Task<List<Claim>> GetByUserIdAsync(Guid userId)
        {
            return await _context.Claims
                .Include(c => c.Documents)
                .Where(c => c.UserId == userId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Claim>> GetAllAsync()
        {
            return await _context.Claims
                .Include(c => c.Documents)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Claim>> GetByPolicyIdAsync(Guid policyId)
        {
            return await _context.Claims
                .Include(c => c.Documents)
                .Where(c => c.PolicyId == policyId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task AddAsync(Claim claim)
        {
            await _context.Claims.AddAsync(claim);
        }

        public async Task UpdateAsync(Claim claim)
        {
            _context.Claims.Update(claim);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
