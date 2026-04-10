using SmartSure.PolicyService.DTOs;
using SmartSure.PolicyService.Models;
using SmartSure.PolicyService.Repositories;

namespace SmartSure.PolicyService.Services
{
    /// <summary>
    /// Represent or implements PaymentService.
    /// </summary>
    public class PaymentService : IPaymentService
    {
        private readonly IPaymentRepository _repo;
        private readonly IPolicyMgmtService _policyService;
        private readonly IRazorpayService _razorpayService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<PaymentService> _logger;

        public PaymentService(
            IPaymentRepository repo, 
            IPolicyMgmtService policyService,
            IRazorpayService razorpayService,
            IConfiguration configuration,
            ILogger<PaymentService> logger)
        {
            _repo = repo;
            _policyService = policyService;
            _razorpayService = razorpayService;
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// Performs the GetByPolicyIdAsync operation.
        /// </summary>
        public async Task<List<PaymentDTO>> GetByPolicyIdAsync(Guid policyId)
        {
            var payments = await _repo.GetByPolicyIdAsync(policyId);
            return payments.Select(p => new PaymentDTO
            {
                PaymentId = p.PaymentId,
                PolicyId = p.PolicyId,
                Amount = p.Amount,
                PaymentDate = p.PaymentDate,
                Status = p.Status,
                PaymentMethod = p.PaymentMethod,
                TransactionReference = p.TransactionReference
            }).ToList();
        }

        /// <summary>
        /// Performs the GetUserPaymentsAsync operation.
        /// </summary>
        public async Task<List<PaymentDTO>> GetUserPaymentsAsync(Guid userId)
        {
            var payments = await _repo.GetByUserIdAsync(userId);
            return payments.Select(p => new PaymentDTO
            {
                PaymentId = p.PaymentId,
                PolicyId = p.PolicyId,
                Amount = p.Amount,
                PaymentDate = p.PaymentDate,
                Status = p.Status,
                PaymentMethod = p.PaymentMethod,
                TransactionReference = p.TransactionReference
            }).ToList();
        }

        /// <summary>
        /// Performs the GetByIdAsync operation.
        /// </summary>
        public async Task<PaymentDTO> GetByIdAsync(Guid paymentId)
        {
            var p = await _repo.GetByIdAsync(paymentId);
            if (p == null) return null;
            return new PaymentDTO
            {
                PaymentId = p.PaymentId,
                PolicyId = p.PolicyId,
                Amount = p.Amount,
                PaymentDate = p.PaymentDate,
                Status = p.Status,
                PaymentMethod = p.PaymentMethod,
                TransactionReference = p.TransactionReference
            };
        }

        /// <summary>
        /// Creates a Razorpay order for payment.
        /// </summary>
        public async Task<RazorpayOrderResponseDTO> CreateRazorpayOrderAsync(CreateRazorpayOrderDTO dto)
        {
            // Generate short receipt (max 40 chars)
            var receipt = $"pol_{dto.PolicyId.ToString().Substring(0, 8)}_{DateTime.UtcNow.Ticks % 1000000}";
            var notes = new Dictionary<string, object>
            {
                { "policy_id", dto.PolicyId.ToString() },
                { "type", "insurance_premium" }
            };

            var order = await _razorpayService.CreateOrderAsync(dto.Amount, dto.Currency, receipt, notes);

            return new RazorpayOrderResponseDTO
            {
                OrderId = order["id"].ToString(),
                Amount = dto.Amount,
                Currency = dto.Currency,
                KeyId = _configuration["Razorpay:KeyId"],
                PolicyId = dto.PolicyId
            };
        }

        /// <summary>
        /// Verifies Razorpay payment signature and records the payment.
        /// </summary>
        public async Task<PaymentDTO> VerifyAndRecordRazorpayPaymentAsync(VerifyRazorpayPaymentDTO dto)
        {
            // Verify signature
            var isValid = _razorpayService.VerifyPaymentSignature(
                dto.RazorpayOrderId, 
                dto.RazorpayPaymentId, 
                dto.RazorpaySignature);

            if (!isValid)
            {
                _logger.LogWarning("Invalid Razorpay signature for policy {PolicyId}", dto.PolicyId);
                throw new UnauthorizedAccessException("Invalid payment signature");
            }

            // Fetch payment details from Razorpay
            var razorpayPayment = await _razorpayService.GetPaymentDetailsAsync(dto.RazorpayPaymentId);
            var amount = Convert.ToDecimal(razorpayPayment["amount"]) / 100; // Convert from paise to rupees
            var status = razorpayPayment["status"].ToString();

            // Record payment in database
            var payment = new Payment
            {
                PaymentId = Guid.NewGuid(),
                PolicyId = dto.PolicyId,
                Amount = amount,
                PaymentDate = DateTime.UtcNow,
                Status = status == "captured" ? "Success" : "Failed",
                PaymentMethod = dto.PaymentMethod,
                TransactionReference = dto.RazorpayPaymentId,
                RazorpayOrderId = dto.RazorpayOrderId,
                RazorpayPaymentId = dto.RazorpayPaymentId
            };

            await _repo.AddAsync(payment);
            await _repo.SaveChangesAsync();

            // Activate policy if payment successful
            if (payment.Status == "Success")
            {
                try
                {
                    await _policyService.ActivatePolicyAsync(dto.PolicyId);
                    _logger.LogInformation("Policy {PolicyId} activated after successful payment", dto.PolicyId);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to activate policy {PolicyId}, might already be active", dto.PolicyId);
                }
            }

            return new PaymentDTO
            {
                PaymentId = payment.PaymentId,
                PolicyId = payment.PolicyId,
                Amount = payment.Amount,
                PaymentDate = payment.PaymentDate,
                Status = payment.Status,
                PaymentMethod = payment.PaymentMethod,
                TransactionReference = payment.TransactionReference
            };
        }

        /// <summary>
        /// Performs the RecordPaymentAsync operation (legacy method).
        /// </summary>
        public async Task<PaymentDTO> RecordPaymentAsync(RecordPaymentDTO dto)
        {
            var payment = new Payment
            {
                PaymentId = Guid.NewGuid(),
                PolicyId = dto.PolicyId,
                Amount = dto.Amount,
                PaymentDate = DateTime.UtcNow,
                Status = "Success", // In real app, this depends on external provider
                PaymentMethod = dto.PaymentMethod,
                TransactionReference = dto.TransactionReference ?? Guid.NewGuid().ToString()
            };

            await _repo.AddAsync(payment);
            await _repo.SaveChangesAsync();

            // After successful payment, activate the policy
            try
            {
                await _policyService.ActivatePolicyAsync(dto.PolicyId);
            }
            catch
            {
                // Policy might already be active – that's OK
            }

            return new PaymentDTO
            {
                PaymentId = payment.PaymentId,
                PolicyId = payment.PolicyId,
                Amount = payment.Amount,
                PaymentDate = payment.PaymentDate,
                Status = payment.Status,
                PaymentMethod = payment.PaymentMethod,
                TransactionReference = payment.TransactionReference
            };
        }

        /// <summary>
        /// Records a failed payment transaction without modifying the policy status.
        /// </summary>
        public async Task<PaymentDTO> RecordFailedPaymentAsync(Guid policyId, decimal amount, string reason)
        {
            var payment = new Payment
            {
                PaymentId = Guid.NewGuid(),
                PolicyId = policyId,
                Amount = amount,
                PaymentDate = DateTime.UtcNow,
                Status = "Failed",
                PaymentMethod = "Razorpay",
                TransactionReference = "FAILED_" + Guid.NewGuid().ToString().Substring(0, 8)
            };

            await _repo.AddAsync(payment);
            await _repo.SaveChangesAsync();

            return new PaymentDTO
            {
                PaymentId = payment.PaymentId,
                PolicyId = payment.PolicyId,
                Amount = payment.Amount,
                PaymentDate = payment.PaymentDate,
                Status = payment.Status,
                PaymentMethod = payment.PaymentMethod,
                TransactionReference = payment.TransactionReference
            };
        }
    }
}
