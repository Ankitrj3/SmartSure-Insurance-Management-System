using SmartSure.PolicyService.DTOs;

namespace SmartSure.PolicyService.Services
{
    /// <summary>
    /// Represent or implements IPaymentService.
    /// </summary>
    public interface IPaymentService
    {
        Task<List<PaymentDTO>> GetByPolicyIdAsync(Guid policyId);
        Task<List<PaymentDTO>> GetUserPaymentsAsync(Guid userId);
        Task<PaymentDTO> GetByIdAsync(Guid paymentId);
        Task<PaymentDTO> RecordPaymentAsync(RecordPaymentDTO dto);
        Task<RazorpayOrderResponseDTO> CreateRazorpayOrderAsync(CreateRazorpayOrderDTO dto);
        Task<PaymentDTO> VerifyAndRecordRazorpayPaymentAsync(VerifyRazorpayPaymentDTO dto);
        Task<PaymentDTO> RecordFailedPaymentAsync(Guid policyId, decimal amount, string reason);
    }
}
