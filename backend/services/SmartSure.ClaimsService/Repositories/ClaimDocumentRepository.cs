using Microsoft.EntityFrameworkCore;
using SmartSure.ClaimsService.Data;
using SmartSure.ClaimsService.Models;

namespace SmartSure.ClaimsService.Repositories
{
    public class ClaimDocumentRepository : IClaimDocumentRepository
    {
        private readonly ClaimsDbContext _context;

        public ClaimDocumentRepository(ClaimsDbContext context)
        {
            _context = context;
        }

        public async Task<ClaimDocument> GetByIdAsync(Guid documentId, Guid claimId)
        {
            return await _context.ClaimDocuments
                .FirstOrDefaultAsync(d => d.DocumentId == documentId && d.ClaimId == claimId);
        }

        public async Task<List<ClaimDocument>> GetByClaimIdAsync(Guid claimId)
        {
            return await _context.ClaimDocuments
                .Where(d => d.ClaimId == claimId)
                .OrderByDescending(d => d.UploadedAt)
                .ToListAsync();
        }

        public async Task AddAsync(ClaimDocument document)
        {
            await _context.ClaimDocuments.AddAsync(document);
        }

        public async Task DeleteAsync(ClaimDocument document)
        {
            _context.ClaimDocuments.Remove(document);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
