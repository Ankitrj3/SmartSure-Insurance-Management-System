using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartSure.PolicyService.DTOs;
using SmartSure.PolicyService.Services;

namespace SmartSure.PolicyService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
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

        [HttpGet("/insurance-types/{typeId}/subtypes")]
        public async Task<IActionResult> GetSubtypes(Guid typeId)
        {
            var subtypes = await _service.GetSubtypesByTypeIdAsync(typeId);
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
    }
}
