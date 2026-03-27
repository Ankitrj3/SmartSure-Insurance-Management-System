using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartSure.PolicyService.DTOs;
using SmartSure.PolicyService.Services;
using SmartSure.Shared.Contracts.Exceptions;

namespace SmartSure.PolicyService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InsuranceController : ControllerBase
    {
        private readonly IInsuranceService _service;

        public InsuranceController(IInsuranceService service)
        {
            _service = service;
        }

        [HttpGet("/insurance-types")]
        public async Task<IActionResult> GetTypes()
        {
            var types = await _service.GetAllTypesAsync();
            return Ok(types);
        }

        [HttpGet("/insurance-types/{typeId}")]
        public async Task<IActionResult> GetType(Guid typeId)
        {
            var type = await _service.GetTypeByIdAsync(typeId);
            if (type == null) return NotFound();
            return Ok(type);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("/insurance-types")]
        public async Task<IActionResult> CreateType(CreateInsuranceTypeDTO dto)
        {
            var type = await _service.CreateTypeAsync(dto);
            return CreatedAtAction(nameof(GetType), new { typeId = type.TypeId }, type);
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("/insurance-types/{typeId}")]
        public async Task<IActionResult> UpdateType(Guid typeId, UpdateInsuranceTypeDTO dto)
        {
            await _service.UpdateTypeAsync(typeId, dto);
            return Ok(new { message = "Insurance type updated successfully" });
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("/insurance-types/{typeId}")]
        public async Task<IActionResult> DeleteType(Guid typeId)
        {
            await _service.DeleteTypeAsync(typeId);
            return Ok(new { message = "Insurance type deleted successfully" });
        }

        [HttpGet("/insurance-types/{typeId}/subtypes")]
        public async Task<IActionResult> GetSubtypesByTypeId(Guid typeId)
        {
            var subtypes = await _service.GetSubtypesByTypeIdAsync(typeId);
            return Ok(subtypes);
        }

        [HttpGet("/insurance-subtypes")]
        public async Task<IActionResult> GetAllSubtypes()
        {
            var subtypes = await _service.GetAllSubtypesAsync();
            return Ok(subtypes);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("/insurance-subtypes")]
        public async Task<IActionResult> CreateSubtype(CreateInsuranceSubtypeDTO dto)
        {
            var subtype = await _service.CreateSubtypeAsync(dto);
            return Ok(subtype);
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("/insurance-subtypes/{subtypeId}")]
        public async Task<IActionResult> UpdateSubtype(Guid subtypeId, UpdateInsuranceSubtypeDTO dto)
        {
            await _service.UpdateSubtypeAsync(subtypeId, dto);
            return Ok(new { message = "Insurance subtype updated successfully" });
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("/insurance-subtypes/{subtypeId}")]
        public async Task<IActionResult> DeleteSubtype(Guid subtypeId)
        {
            await _service.DeleteSubtypeAsync(subtypeId);
            return Ok(new { message = "Insurance subtype deleted successfully" });
        }
    }
}
