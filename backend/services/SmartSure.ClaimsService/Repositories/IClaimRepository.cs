using SmartSure.ClaimsService.Models;

namespace SmartSure.ClaimsService.Repositories
{
    /// <summary>
    /// Represent or implements IClaimRepository.
    /// </summary>
    public interface IClaimRepository
    {
        Task<Claim> GetByIdAsync(Guid claimId);
        Task<List<Claim>> GetByUserIdAsync(Guid userId);
        Task<List<Claim>> GetAllAsync();
        Task<List<Claim>> GetByPolicyIdAsync(Guid policyId);
        Task AddAsync(Claim claim);
        Task UpdateAsync(Claim claim);
        Task SaveChangesAsync();
    }
}
