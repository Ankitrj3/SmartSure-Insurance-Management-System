using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartSure.PolicyService.DTOs;
using SmartSure.PolicyService.Services;
using SmartSure.Shared.Contracts.Exceptions;

namespace SmartSure.PolicyService.Controllers
{
    /// <summary>
    /// Represent or implements PaymentsController.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PaymentsController : ControllerBase
    {
        private readonly IPaymentService _service;

        public PaymentsController(IPaymentService service)
        {
            _service = service;
        }

        /// <summary>
        /// Performs the GetPayments operation.
        /// </summary>
        [HttpGet("/policies/{policyId}/payments")]
        public async Task<IActionResult> GetPayments(Guid policyId)
        {
            var payments = await _service.GetByPolicyIdAsync(policyId);
            return Ok(payments);
        }

        /// <summary>
        /// Performs the GetPayment operation.
        /// </summary>
        [HttpGet("/payments/{paymentId}")] // To keep the GetPayment by ID working if it's called
        public async Task<IActionResult> GetPayment(Guid paymentId)
        {
            var payment = await _service.GetByIdAsync(paymentId);
            if (payment == null) return NotFound();
            return Ok(payment);
        }

        /// <summary>
        /// Performs the RecordPayment operation.
        /// </summary>
        [HttpPost("/policies/{policyId}/payments")]
        public async Task<IActionResult> RecordPayment(Guid policyId, [FromBody] RecordPaymentDTO dto)
        {
            dto.PolicyId = policyId;
            var payment = await _service.RecordPaymentAsync(dto);
            return CreatedAtAction(nameof(GetPayment), new { paymentId = payment.PaymentId }, payment);
        }
    }
}
