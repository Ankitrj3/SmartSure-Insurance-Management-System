using System.ComponentModel.DataAnnotations;

namespace SmartSure.PolicyService.DTOs
{
    /// <summary>
    /// Represent or implements PaymentDTO.
    /// </summary>
    public class PaymentDTO
    {
        public Guid PaymentId { get; set; }
        public Guid PolicyId { get; set; }
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; }
        public string Status { get; set; }
        public string PaymentMethod { get; set; }
        public string TransactionReference { get; set; }
    }

    /// <summary>
    /// Represent or implements RecordPaymentDTO.
    /// </summary>
    public class RecordPaymentDTO
    {
        [Required]
        public Guid PolicyId { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal Amount { get; set; }

        [Required]
        public string PaymentMethod { get; set; }

        public string? TransactionReference { get; set; }
    }

    /// <summary>
    /// DTO for creating Razorpay order.
    /// </summary>
    public class CreateRazorpayOrderDTO
    {
        [Required]
        public Guid PolicyId { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal Amount { get; set; }

        public string Currency { get; set; } = "INR";
    }

    /// <summary>
    /// DTO for verifying Razorpay payment.
    /// </summary>
    public class VerifyRazorpayPaymentDTO
    {
        [Required]
        public Guid PolicyId { get; set; }

        [Required]
        public string RazorpayOrderId { get; set; }

        [Required]
        public string RazorpayPaymentId { get; set; }

        [Required]
        public string RazorpaySignature { get; set; }

        [Required]
        public string PaymentMethod { get; set; }
    }

    /// <summary>
    /// Response DTO for Razorpay order creation.
    /// </summary>
    public class RazorpayOrderResponseDTO
    {
        public string OrderId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public string KeyId { get; set; }
        public Guid PolicyId { get; set; }
    }

    /// <summary>
    /// DTO for recording a failed payment attempt.
    /// </summary>
    public class RecordFailedPaymentDTO
    {
        public Guid PolicyId { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal Amount { get; set; }

        public string? Reason { get; set; }
    }
}
