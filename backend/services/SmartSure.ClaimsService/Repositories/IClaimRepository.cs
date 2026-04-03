using SmartSure.ClaimsService.Models;
using SmartSure.Shared.Contracts.DTOs;

namespace SmartSure.ClaimsService.Repositories
{
    /// <summary>
    /// Represent or implements IClaimRepository.
    /// </summary>
    public interface IClaimRepository
    {
        Task<Claim> GetByIdAsync(Guid claimId);
        Task<PagedResult<Claim>> GetByUserIdAsync(Guid userId, int page = 1, int pageSize = 10);
        Task<PagedResult<Claim>> GetAllAsync(int page = 1, int pageSize = 10);
        Task<List<Claim>> GetByPolicyIdAsync(Guid policyId);
        Task AddAsync(Claim claim);
        Task UpdateAsync(Claim claim);
        Task SaveChangesAsync();
    }
}
