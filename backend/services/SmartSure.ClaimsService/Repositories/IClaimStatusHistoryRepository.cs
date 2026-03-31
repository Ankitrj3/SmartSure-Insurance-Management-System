using SmartSure.ClaimsService.Models;

namespace SmartSure.ClaimsService.Repositories
{
    /// <summary>
    /// Represent or implements IClaimStatusHistoryRepository.
    /// </summary>
    public interface IClaimStatusHistoryRepository
    {
        Task<List<ClaimStatusHistory>> GetByClaimIdAsync(Guid claimId);
        Task AddAsync(ClaimStatusHistory history);
        Task SaveChangesAsync();
    }
}
