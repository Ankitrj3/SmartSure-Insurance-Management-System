using SmartSure.PolicyService.DTOs;

namespace SmartSure.PolicyService.Services
{
    /// <summary>
    /// Represent or implements IPaymentService.
    /// </summary>
    public interface IPaymentService
    {
        Task<List<PaymentDTO>> GetByPolicyIdAsync(Guid policyId);
        Task<PaymentDTO> GetByIdAsync(Guid paymentId);
        Task<PaymentDTO> RecordPaymentAsync(RecordPaymentDTO dto);
        Task<RazorpayOrderResponseDTO> CreateRazorpayOrderAsync(CreateRazorpayOrderDTO dto);
        Task<PaymentDTO> VerifyAndRecordRazorpayPaymentAsync(VerifyRazorpayPaymentDTO dto);
    }
}
