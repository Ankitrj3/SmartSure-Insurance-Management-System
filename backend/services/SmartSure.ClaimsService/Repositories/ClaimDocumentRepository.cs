using Microsoft.EntityFrameworkCore;
using SmartSure.ClaimsService.Data;
using SmartSure.ClaimsService.Models;

namespace SmartSure.ClaimsService.Repositories
{
    /// <summary>
    /// Represent or implements ClaimDocumentRepository.
    /// </summary>
    public class ClaimDocumentRepository : IClaimDocumentRepository
    {
        private readonly ClaimsDbContext _context;

        public ClaimDocumentRepository(ClaimsDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Performs the GetByIdAsync operation.
        /// </summary>
        public async Task<ClaimDocument> GetByIdAsync(Guid documentId, Guid claimId)
        {
            return await _context.ClaimDocuments
                .FirstOrDefaultAsync(d => d.DocumentId == documentId && d.ClaimId == claimId);
        }

        /// <summary>
        /// Performs the GetByClaimIdAsync operation.
        /// </summary>
        public async Task<List<ClaimDocument>> GetByClaimIdAsync(Guid claimId)
        {
            return await _context.ClaimDocuments
                .Where(d => d.ClaimId == claimId)
                .OrderByDescending(d => d.UploadedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Performs the AddAsync operation.
        /// </summary>
        public async Task AddAsync(ClaimDocument document)
        {
            await _context.ClaimDocuments.AddAsync(document);
        }

        /// <summary>
        /// Performs the DeleteAsync operation.
        /// </summary>
        public async Task DeleteAsync(ClaimDocument document)
        {
            _context.ClaimDocuments.Remove(document);
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
