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
        private readonly IRazorpayService _razorpayService;

        public PaymentsController(IPaymentService service, IRazorpayService razorpayService)
        {
            _service = service;
            _razorpayService = razorpayService;
        }

        private Guid GetUserId()
        {
            var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdStr, out var userId))
                throw new UnauthorizedAccessException("User context is missing or invalid.");
            return userId;
        }

        /// <summary>
        /// Performs the GetMyPayments operation.
        /// </summary>
        [HttpGet("/payments")]
        public async Task<IActionResult> GetMyPayments()
        {
            var userId = GetUserId();
            var payments = await _service.GetUserPaymentsAsync(userId);
            return Ok(payments);
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
        /// Records a failed payment attempt without modifying the policy status.
        /// Called from the frontend when a Razorpay payment is cancelled or declined.
        /// </summary>
        [HttpPost("/policies/{policyId}/payments/failed")]
        public async Task<IActionResult> RecordFailedPayment(Guid policyId, [FromBody] RecordFailedPaymentDTO dto)
        {
            dto.PolicyId = policyId;
            var payment = await _service.RecordFailedPaymentAsync(dto.PolicyId, dto.Amount, dto.Reason ?? "Payment cancelled or declined");
            return Ok(payment);
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
        /// Creates a Razorpay order for payment.
        /// </summary>
        [HttpPost("/policies/{policyId}/payments/razorpay/create-order")]
        public async Task<IActionResult> CreateRazorpayOrder(Guid policyId, [FromBody] CreateRazorpayOrderDTO dto)
        {
            dto.PolicyId = policyId;
            var orderResponse = await _service.CreateRazorpayOrderAsync(dto);
            return Ok(orderResponse);
        }

        /// <summary>
        /// Verifies and records Razorpay payment.
        /// </summary>
        [HttpPost("/policies/{policyId}/payments/razorpay/verify")]
        public async Task<IActionResult> VerifyRazorpayPayment(Guid policyId, [FromBody] VerifyRazorpayPaymentDTO dto)
        {
            dto.PolicyId = policyId;
            var payment = await _service.VerifyAndRecordRazorpayPaymentAsync(dto);
            return Ok(payment);
        }

        /// <summary>
        /// Performs the RecordPayment operation (legacy endpoint).
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
