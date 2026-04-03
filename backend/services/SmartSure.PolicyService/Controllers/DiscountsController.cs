using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartSure.PolicyService.DTOs;
using SmartSure.PolicyService.Services;
using System.Security.Claims;
using SmartSure.Shared.Contracts.Exceptions;

namespace SmartSure.PolicyService.Controllers
{
    /// <summary>
    /// Represent or implements DiscountsController.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class DiscountsController : ControllerBase
    {
        private readonly IDiscountService _service;

        public DiscountsController(IDiscountService service)
        {
            _service = service;
        }

        /// <summary>
        /// Get all active discount coupons (public for display).
        /// </summary>
        [HttpGet("/discounts")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll()
        {
            var discounts = await _service.GetAllDiscountsAsync();
            return Ok(discounts);
        }

        /// <summary>
        /// Performs the GetById operation.
        /// </summary>
        [HttpGet("/discounts/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var discount = await _service.GetDiscountByIdAsync(id);
            if (discount == null) return NotFound();
            return Ok(discount);
        }

        /// <summary>
        /// Performs the Create operation.
        /// </summary>
        [HttpPost("/discounts")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] CreateDiscountDTO dto)
        {
            if (!ModelState.IsValid) 
            {
               return BadRequest(ModelState);
            }
            var result = await _service.CreateDiscountAsync(dto);
            return Ok(result);
        }

        /// <summary>
        /// Performs the Update operation.
        /// </summary>
        [HttpPut("/discounts/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(Guid id, [FromBody] CreateDiscountDTO dto)
        {
            try
            {
                await _service.UpdateDiscountAsync(id, dto);
                return Ok(new { message = "Discount updated successfully" });
            }
            catch (SmartSureException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new BusinessRuleException(ex.Message);
            }
        }

        /// <summary>
        /// Performs the Delete operation.
        /// </summary>
        [HttpDelete("/discounts/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _service.DeleteDiscountAsync(id);
            return Ok(new { message = "Discount deleted successfully" });
        }

        /// <summary>
        /// Calculate applicable discounts (first-time + coupon) for a premium amount.
        /// </summary>
        [HttpPost("/discounts/calculate")]
        [Authorize]
        public async Task<IActionResult> Calculate([FromBody] CalculateDiscountRequest request)
        {
            var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdStr, out var userId))
                return Unauthorized();

            var result = await _service.CalculateDiscountAsync(userId, request.OriginalPremium, request.CouponCode);
            return Ok(result);
        }
    }

    /// <summary>
    /// Represent or implements CalculateDiscountRequest.
    /// </summary>
    public class CalculateDiscountRequest
    {
        public decimal OriginalPremium { get; set; }
        public string? CouponCode { get; set; }
    }
}
