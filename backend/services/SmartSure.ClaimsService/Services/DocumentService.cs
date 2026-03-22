using Microsoft.EntityFrameworkCore;
using SmartSure.ClaimsService.Data;
using SmartSure.ClaimsService.DTOs;
using SmartSure.ClaimsService.Models;
using CG.Web.MegaApiClient;
using Microsoft.AspNetCore.Http;

namespace SmartSure.ClaimsService.Services
{
    public class DocumentService : IDocumentService
    {
        private readonly ClaimsDbContext _context;
        private readonly ILogger<DocumentService> _logger;
        private readonly IConfiguration _config;

        public DocumentService(ClaimsDbContext context, ILogger<DocumentService> logger, IConfiguration config)
        {
            _context = context;
            _logger = logger;
            _config = config;
        }

        public async Task<DocumentResponseDTO> AddDocumentAsync(Guid claimId, IFormFile file)
        {
            var claim = await _context.Claims.FindAsync(claimId);
            if (claim == null) throw new KeyNotFoundException("Claim not found.");

            string megaEmail = _config["Mega:Email"];
            string megaPassword = _config["Mega:Password"];
            
            if (string.IsNullOrEmpty(megaEmail) || string.IsNullOrEmpty(megaPassword))
                throw new InvalidOperationException("MEGA.nz credentials are not configured in appsettings.");

            var megaClient = new MegaApiClient();
            megaClient.Login(megaEmail, megaPassword);

            // Fetch the root folder
            var nodes = megaClient.GetNodes();
            var root = nodes.Single(n => n.Type == NodeType.Root);
            
            // Upload the file directly from the stream into MEGA
            using var stream = file.OpenReadStream();
            var uploadedNode = await megaClient.UploadAsync(stream, file.FileName, root);

            // Get the public download link for the file
            var downloadLink = megaClient.GetDownloadLink(uploadedNode);
            megaClient.Logout();

            var document = new ClaimDocument
            {
                ClaimId = claimId,
                FileName = file.FileName,
                FileUrl = downloadLink.ToString(), // Mega.nz URL
                ContentType = file.ContentType,
                FileSize = file.Length
            };

            _context.ClaimDocuments.Add(document);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Document {FileName} uploaded to Mega.nz for claim {ClaimId}. URL: {FileUrl}", file.FileName, claimId, document.FileUrl);

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
