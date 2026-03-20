using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartSure.ClaimsService.DTOs;
using SmartSure.ClaimsService.Services;

namespace SmartSure.ClaimsService.Controllers
{
    [ApiController]
    [Route("claims/{claimId}/documents")]
    [Authorize]
    public class ClaimDocumentsController : ControllerBase
    {
        private readonly IDocumentService _documentService;

        public ClaimDocumentsController(IDocumentService documentService)
        {
            _documentService = documentService;
        }

        [HttpGet]
        public async Task<IActionResult> GetDocuments(Guid claimId)
        {
            var documents = await _documentService.GetDocumentsAsync(claimId);
            return Ok(documents);
        }

        [HttpPost]
        public async Task<IActionResult> UploadDocument(Guid claimId, [FromBody] DocumentUploadDTO dto)
        {
            var document = await _documentService.AddDocumentAsync(claimId, dto);
            return CreatedAtAction(nameof(GetDocuments), new { claimId }, document);
        }

        [HttpDelete("{docId}")]
        public async Task<IActionResult> DeleteDocument(Guid claimId, Guid docId)
        {
            await _documentService.DeleteDocumentAsync(claimId, docId);
            return Ok(new { message = "Document deleted successfully" });
        }
    }
}
