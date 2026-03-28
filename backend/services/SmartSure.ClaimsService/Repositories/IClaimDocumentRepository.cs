using SmartSure.ClaimsService.Models;

namespace SmartSure.ClaimsService.Repositories
{
    public interface IClaimDocumentRepository
    {
        Task<ClaimDocument> GetByIdAsync(Guid documentId, Guid claimId);
        Task<List<ClaimDocument>> GetByClaimIdAsync(Guid claimId);
        Task AddAsync(ClaimDocument document);
        Task DeleteAsync(ClaimDocument document);
        Task SaveChangesAsync();
    }
}
