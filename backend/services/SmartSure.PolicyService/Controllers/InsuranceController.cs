using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartSure.PolicyService.DTOs;
using SmartSure.PolicyService.Services;
using SmartSure.Shared.Contracts.Exceptions;

namespace SmartSure.PolicyService.Controllers
{
    /// <summary>
    /// Represent or implements InsuranceController.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class InsuranceController : ControllerBase
    {
        private readonly IInsuranceService _service;

        public InsuranceController(IInsuranceService service)
        {
            _service = service;
        }

        /// <summary>
        /// Performs the GetTypes operation.
        /// </summary>
        [HttpGet("/insurance-types")]
        public async Task<IActionResult> GetTypes()
        {
            var types = await _service.GetAllTypesAsync();
            return Ok(types);
        }

        /// <summary>
        /// Performs the GetType operation.
        /// </summary>
        [HttpGet("/insurance-types/{typeId}")]
        public async Task<IActionResult> GetType(Guid typeId)
        {
            var type = await _service.GetTypeByIdAsync(typeId);
            if (type == null) return NotFound();
            return Ok(type);
        }

        /// <summary>
        /// Performs the CreateType operation.
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpPost("/insurance-types")]
        public async Task<IActionResult> CreateType(CreateInsuranceTypeDTO dto)
        {
            var type = await _service.CreateTypeAsync(dto);
            return CreatedAtAction(nameof(GetType), new { typeId = type.TypeId }, type);
        }

        /// <summary>
        /// Performs the UpdateType operation.
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpPut("/insurance-types/{typeId}")]
        public async Task<IActionResult> UpdateType(Guid typeId, UpdateInsuranceTypeDTO dto)
        {
            await _service.UpdateTypeAsync(typeId, dto);
            return Ok(new { message = "Insurance type updated successfully" });
        }

        /// <summary>
        /// Performs the DeleteType operation.
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpDelete("/insurance-types/{typeId}")]
        public async Task<IActionResult> DeleteType(Guid typeId)
        {
            await _service.DeleteTypeAsync(typeId);
            return Ok(new { message = "Insurance type deleted successfully" });
        }

        /// <summary>
        /// Performs the GetSubtypesByTypeId operation.
        /// </summary>
        [HttpGet("/insurance-types/{typeId}/subtypes")]
        public async Task<IActionResult> GetSubtypesByTypeId(Guid typeId)
        {
            var subtypes = await _service.GetSubtypesByTypeIdAsync(typeId);
            return Ok(subtypes);
        }

        /// <summary>
        /// Performs the GetAllSubtypes operation.
        /// </summary>
        [HttpGet("/insurance-subtypes")]
        public async Task<IActionResult> GetAllSubtypes()
        {
            var subtypes = await _service.GetAllSubtypesAsync();
            return Ok(subtypes);
        }

        /// <summary>
        /// Performs the CreateSubtype operation.
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpPost("/insurance-subtypes")]
        public async Task<IActionResult> CreateSubtype(CreateInsuranceSubtypeDTO dto)
        {
            var subtype = await _service.CreateSubtypeAsync(dto);
            return Ok(subtype);
        }

        /// <summary>
        /// Performs the UpdateSubtype operation.
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpPut("/insurance-subtypes/{subtypeId}")]
        public async Task<IActionResult> UpdateSubtype(Guid subtypeId, UpdateInsuranceSubtypeDTO dto)
        {
            await _service.UpdateSubtypeAsync(subtypeId, dto);
            return Ok(new { message = "Insurance subtype updated successfully" });
        }

        /// <summary>
        /// Performs the DeleteSubtype operation.
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpDelete("/insurance-subtypes/{subtypeId}")]
        public async Task<IActionResult> DeleteSubtype(Guid subtypeId)
        {
            await _service.DeleteSubtypeAsync(subtypeId);
            return Ok(new { message = "Insurance subtype deleted successfully" });
        }
    }
}
