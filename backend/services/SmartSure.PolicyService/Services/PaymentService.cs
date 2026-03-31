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

        public PaymentService(IPaymentRepository repo, IPolicyMgmtService policyService)
        {
            _repo = repo;
            _policyService = policyService;
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
        /// Performs the RecordPaymentAsync operation.
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
    }
}
