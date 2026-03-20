using Microsoft.EntityFrameworkCore;
using SmartSure.ClaimsService.Data;
using SmartSure.ClaimsService.DTOs;
using SmartSure.ClaimsService.Models;

namespace SmartSure.ClaimsService.Services
{
    public class DocumentService : IDocumentService
    {
        private readonly ClaimsDbContext _context;
        private readonly ILogger<DocumentService> _logger;

        public DocumentService(ClaimsDbContext context, ILogger<DocumentService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<DocumentResponseDTO> AddDocumentAsync(Guid claimId, DocumentUploadDTO dto)
        {
            var claim = await _context.Claims.FindAsync(claimId);
            if (claim == null) throw new KeyNotFoundException("Claim not found.");

            var document = new ClaimDocument
            {
                DocumentId = Guid.NewGuid(),
                ClaimId = claimId,
                FileName = dto.FileName,
                FileUrl = dto.FileUrl,     // Mega.nz URL
                ContentType = dto.ContentType,
                FileSize = dto.FileSize
            };

            _context.ClaimDocuments.Add(document);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Document {FileName} added to claim {ClaimId}. URL: {FileUrl}", dto.FileName, claimId, dto.FileUrl);

            return new DocumentResponseDTO
            {
                DocumentId = document.DocumentId,
                ClaimId = document.ClaimId,
                FileName = document.FileName,
                FileUrl = document.FileUrl,
                ContentType = document.ContentType,
                FileSize = document.FileSize,
                UploadedAt = document.UploadedAt
            };
        }

        public async Task<List<DocumentResponseDTO>> GetDocumentsAsync(Guid claimId)
        {
            var documents = await _context.ClaimDocuments
                .Where(d => d.ClaimId == claimId)
                .OrderByDescending(d => d.UploadedAt)
                .ToListAsync();

            return documents.Select(d => new DocumentResponseDTO
            {
                DocumentId = d.DocumentId,
                ClaimId = d.ClaimId,
                FileName = d.FileName,
                FileUrl = d.FileUrl,
                ContentType = d.ContentType,
                FileSize = d.FileSize,
                UploadedAt = d.UploadedAt
            }).ToList();
        }

        public async Task DeleteDocumentAsync(Guid claimId, Guid documentId)
        {
            var document = await _context.ClaimDocuments
                .FirstOrDefaultAsync(d => d.DocumentId == documentId && d.ClaimId == claimId);

            if (document == null) throw new KeyNotFoundException("Document not found.");

            _context.ClaimDocuments.Remove(document);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Document {DocumentId} deleted from claim {ClaimId}", documentId, claimId);
        }
    }
}
