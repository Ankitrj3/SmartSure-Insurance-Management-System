using Microsoft.AspNetCore.Http;
using SmartSure.ClaimsService.DTOs;

namespace SmartSure.ClaimsService.Services
{
    /// <summary>
    /// Represent or implements IDocumentService.
    /// </summary>
    public interface IDocumentService
    {
        Task<DocumentResponseDTO> AddDocumentAsync(Guid claimId, IFormFile file);
        Task<List<DocumentResponseDTO>> GetDocumentsAsync(Guid claimId);
        Task DeleteDocumentAsync(Guid claimId, Guid documentId);
    }
}
