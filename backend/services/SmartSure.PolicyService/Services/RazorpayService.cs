using Razorpay.Api;

namespace SmartSure.PolicyService.Services
{
    /// <summary>
    /// Service for Razorpay payment gateway integration.
    /// </summary>
    public class RazorpayService : IRazorpayService
    {
        private readonly RazorpayClient _client;
        private readonly ILogger<RazorpayService> _logger;

        public RazorpayService(IConfiguration configuration, ILogger<RazorpayService> logger)
        {
            var keyId = configuration["Razorpay:KeyId"];
            var keySecret = configuration["Razorpay:KeySecret"];
            
            if (string.IsNullOrEmpty(keyId) || string.IsNullOrEmpty(keySecret))
            {
                throw new InvalidOperationException("Razorpay credentials not configured");
            }

            _client = new RazorpayClient(keyId, keySecret);
            _logger = logger;
        }

        /// <summary>
        /// Creates a Razorpay order for payment.
        /// </summary>
        public async Task<Dictionary<string, object>> CreateOrderAsync(decimal amount, string currency, string receipt, Dictionary<string, object>? notes = null)
        {
            try
            {
                var options = new Dictionary<string, object>
                {
                    { "amount", (int)(amount * 100) }, // Amount in paise
                    { "currency", currency },
                    { "receipt", receipt },
                    { "payment_capture", 1 } // Auto capture
                };

                if (notes != null && notes.Count > 0)
                {
                    options.Add("notes", notes);
                }

                var order = _client.Order.Create(options);
                
                // Convert Razorpay Order to Dictionary
                var result = new Dictionary<string, object>
                {
                    { "id", order["id"] },
                    { "amount", order["amount"] },
                    { "currency", order["currency"] },
                    { "receipt", order["receipt"] },
                    { "status", order["status"] }
                };
                
                string orderId = order["id"]?.ToString() ?? string.Empty;
                _logger.LogInformation("Razorpay order created: {OrderId}", orderId);
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create Razorpay order");
                throw;
            }
        }

        /// <summary>
        /// Verifies Razorpay payment signature.
        /// </summary>
        public bool VerifyPaymentSignature(string orderId, string paymentId, string signature)
        {
            try
            {
                var attributes = new Dictionary<string, string>
                {
                    { "razorpay_order_id", orderId },
                    { "razorpay_payment_id", paymentId },
                    { "razorpay_signature", signature }
                };

                Utils.verifyPaymentSignature(attributes);
                
                _logger.LogInformation("Payment signature verified for order: {OrderId}", orderId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Payment signature verification failed for order: {OrderId}", orderId);
                return false;
            }
        }

        /// <summary>
        /// Fetches payment details from Razorpay.
        /// </summary>
        public async Task<Dictionary<string, object>> GetPaymentDetailsAsync(string paymentId)
        {
            try
            {
                var payment = _client.Payment.Fetch(paymentId);
                _logger.LogInformation("Fetched payment details: {PaymentId}", paymentId);
                return new Dictionary<string, object>
                {
                    { "amount", payment["amount"] },
                    { "status", payment["status"] }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch payment details: {PaymentId}", paymentId);
                throw;
            }
        }

        /// <summary>
        /// Refunds a payment.
        /// </summary>
        public async Task<Refund> RefundPaymentAsync(string paymentId, decimal? amount = null)
        {
            try
            {
                var options = new Dictionary<string, object>();
                
                if (amount.HasValue)
                {
                    options.Add("amount", (int)(amount.Value * 100)); // Amount in paise
                }

                var refund = _client.Payment.Fetch(paymentId).Refund(options);
                
                _logger.LogInformation("Refund initiated for payment: {PaymentId}", paymentId);
                return refund;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to refund payment: {PaymentId}", paymentId);
                throw;
            }
        }
    }
}
