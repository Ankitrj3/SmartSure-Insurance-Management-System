using Microsoft.EntityFrameworkCore;
using SmartSure.Shared.Contracts.DTOs;
using SmartSure.Shared.Contracts.Extensions;
using SmartSure.ClaimsService.Data;
using SmartSure.ClaimsService.Models;

namespace SmartSure.ClaimsService.Repositories
{
    /// <summary>
    /// Represent or implements ClaimRepository.
    /// </summary>
    public class ClaimRepository : IClaimRepository
    {
        private readonly ClaimsDbContext _context;

        public ClaimRepository(ClaimsDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Performs the GetByIdAsync operation.
        /// </summary>
        public async Task<Claim> GetByIdAsync(Guid claimId)
        {
            return await _context.Claims
                .Include(c => c.Documents)
                .Include(c => c.StatusHistory)
                .FirstOrDefaultAsync(c => c.ClaimId == claimId);
        }

        /// <summary>
        /// Performs the GetByUserIdAsync operation.
        /// </summary>
        public async Task<PagedResult<Claim>> GetByUserIdAsync(Guid userId, int page = 1, int pageSize = 10)
        {
            return await _context.Claims
                .Include(c => c.Documents)
                .Where(c => c.UserId == userId)
                .OrderByDescending(c => c.CreatedAt)
                .ToPagedResultAsync(page, pageSize);
        }

        /// <summary>
        /// Performs the GetAllAsync operation.
        /// </summary>
        public async Task<PagedResult<Claim>> GetAllAsync(int page = 1, int pageSize = 10)
        {
            return await _context.Claims
                .Include(c => c.Documents)
                .OrderByDescending(c => c.CreatedAt)
                .ToPagedResultAsync(page, pageSize);
        }

        /// <summary>
        /// Performs the GetByPolicyIdAsync operation.
        /// </summary>
        public async Task<List<Claim>> GetByPolicyIdAsync(Guid policyId)
        {
            return await _context.Claims
                .Include(c => c.Documents)
                .Where(c => c.PolicyId == policyId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Performs the AddAsync operation.
        /// </summary>
        public async Task AddAsync(Claim claim)
        {
            await _context.Claims.AddAsync(claim);
        }

        /// <summary>
        /// Performs the UpdateAsync operation.
        /// </summary>
        public async Task UpdateAsync(Claim claim)
        {
            _context.Claims.Update(claim);
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
