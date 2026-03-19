using System.ComponentModel.DataAnnotations;

namespace SmartSure.PolicyService.DTOs
{
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

    public class RecordPaymentDTO
    {
        [Required]
        public Guid PolicyId { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal Amount { get; set; }

        [Required]
        public string PaymentMethod { get; set; }

        public string TransactionReference { get; set; }
    }
}
