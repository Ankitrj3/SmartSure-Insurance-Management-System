using Razorpay.Api;

namespace SmartSure.PolicyService.Services
{
    /// <summary>
    /// Interface for Razorpay payment gateway integration.
    /// </summary>
    public interface IRazorpayService
    {
        Task<Dictionary<string, object>> CreateOrderAsync(decimal amount, string currency, string receipt, Dictionary<string, object>? notes = null);
        bool VerifyPaymentSignature(string orderId, string paymentId, string signature);
        Task<Dictionary<string, object>> GetPaymentDetailsAsync(string paymentId);
        Task<Refund> RefundPaymentAsync(string paymentId, decimal? amount = null);
    }
}
