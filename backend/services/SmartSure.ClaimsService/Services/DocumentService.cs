using SmartSure.ClaimsService.DTOs;
using SmartSure.ClaimsService.Models;
using SmartSure.ClaimsService.Repositories;
using CG.Web.MegaApiClient;
using SmartSure.Shared.Contracts.Exceptions;

namespace SmartSure.ClaimsService.Services
{
    /// <summary>
    /// Represent or implements DocumentService.
    /// </summary>
    public class DocumentService : IDocumentService
    {
        private readonly IClaimRepository _claimRepository;
        private readonly IClaimDocumentRepository _documentRepository;
        private readonly ILogger<DocumentService> _logger;
        private readonly IConfiguration _config;

        public DocumentService(
            IClaimRepository claimRepository,
            IClaimDocumentRepository documentRepository,
            ILogger<DocumentService> logger, 
            IConfiguration config)
        {
            _claimRepository = claimRepository;
            _documentRepository = documentRepository;
            _logger = logger;
            _config = config;
        }

        /// <summary>
        /// Performs the AddDocumentAsync operation.
        /// </summary>
        public async Task<DocumentResponseDTO> AddDocumentAsync(Guid claimId, IFormFile file)
        {
            var claim = await _claimRepository.GetByIdAsync(claimId);
            if (claim == null) throw new NotFoundException("Claim", claimId);

            string? megaEmail    = _config["Mega:Email"];
            string? megaPassword = _config["Mega:Password"];

            if (string.IsNullOrEmpty(megaEmail) || string.IsNullOrEmpty(megaPassword))
                throw new ValidationException("MEGA.nz credentials are not configured in appsettings.");

            var megaClient = new MegaApiClient();
            megaClient.Login(megaEmail, megaPassword);

            var nodes = megaClient.GetNodes();
            var root  = nodes.Single(n => n.Type == NodeType.Root);

            using var stream     = file.OpenReadStream();
            var uploadedNode     = await megaClient.UploadAsync(stream, file.FileName, root);
            var downloadLink     = megaClient.GetDownloadLink(uploadedNode);
            megaClient.Logout();

            var document = new ClaimDocument
            {
                ClaimId     = claimId,
                FileName    = file.FileName,
                FileUrl     = downloadLink.ToString(),
                ContentType = file.ContentType,
                FileSize    = file.Length
            };

            await _documentRepository.AddAsync(document);
            await _documentRepository.SaveChangesAsync();

            _logger.LogInformation("Document {FileName} uploaded to Mega.nz for claim {ClaimId}. URL: {FileUrl}",
                file.FileName, claimId, document.FileUrl);

            return new DocumentResponseDTO
            {
                DocumentId  = document.DocumentId,
                ClaimId     = document.ClaimId,
                FileName    = document.FileName,
                FileUrl     = document.FileUrl,
                ContentType = document.ContentType,
                FileSize    = document.FileSize,
                UploadedAt  = document.UploadedAt
            };
        }

        /// <summary>
        /// Performs the GetDocumentsAsync operation.
        /// </summary>
        public async Task<List<DocumentResponseDTO>> GetDocumentsAsync(Guid claimId)
        {
            var documents = await _documentRepository.GetByClaimIdAsync(claimId);

            return documents.Select(d => new DocumentResponseDTO
            {
                DocumentId  = d.DocumentId,
                ClaimId     = d.ClaimId,
                FileName    = d.FileName,
                FileUrl     = d.FileUrl,
                ContentType = d.ContentType,
                FileSize    = d.FileSize,
                UploadedAt  = d.UploadedAt
            }).ToList();
        }

        /// <summary>
        /// Performs the DeleteDocumentAsync operation.
        /// </summary>
        public async Task DeleteDocumentAsync(Guid claimId, Guid documentId)
        {
            var document = await _documentRepository.GetByIdAsync(documentId, claimId);
            if (document == null) throw new NotFoundException("Document", documentId);

            await _documentRepository.DeleteAsync(document);
            await _documentRepository.SaveChangesAsync();

            _logger.LogInformation("Document {DocumentId} deleted from claim {ClaimId}", documentId, claimId);
        }
    }
}
