using SmartSure.ClaimsService.DTOs;

namespace SmartSure.ClaimsService.Services
{
    public interface IDocumentService
    {
        Task<DocumentResponseDTO> AddDocumentAsync(Guid claimId, DocumentUploadDTO dto);
        Task<List<DocumentResponseDTO>> GetDocumentsAsync(Guid claimId);
        Task DeleteDocumentAsync(Guid claimId, Guid documentId);
    }
}
