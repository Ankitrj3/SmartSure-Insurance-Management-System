using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartSure.ClaimsService.Services;
using SmartSure.Shared.Contracts.Exceptions;

namespace SmartSure.ClaimsService.Controllers
{
    /// <summary>
    /// Represent or implements ClaimDocumentsController.
    /// </summary>
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

        /// <summary>
        /// Performs the GetDocuments operation.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetDocuments(Guid claimId)
        {
            var documents = await _documentService.GetDocumentsAsync(claimId);
            return Ok(documents);
        }

        /// <summary>
        /// Performs the UploadDocument operation.
        /// </summary>
        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadDocument(Guid claimId, IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { message = "No file provided." });

            var document = await _documentService.AddDocumentAsync(claimId, file);
            return CreatedAtAction(nameof(GetDocuments), new { claimId }, document);
        }

        /// <summary>
        /// Performs the DeleteDocument operation.
        /// </summary>
        [HttpDelete("{docId}")]
        public async Task<IActionResult> DeleteDocument(Guid claimId, Guid docId)
        {
            await _documentService.DeleteDocumentAsync(claimId, docId);
            return Ok(new { message = "Document deleted successfully" });
        }
    }
}
