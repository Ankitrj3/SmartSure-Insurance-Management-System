using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartSure.PolicyService.Models
{
    /// <summary>
    /// Represent or implements Discount.
    /// </summary>
    public class Discount
    {
        [Key]
        public Guid DiscountId { get; set; }

        [Required]
        [StringLength(50)]
        public string Code { get; set; }

        [Required]
        [StringLength(200)]
        public string Description { get; set; }

        /// <summary>
        /// Discount percentage (e.g. 10 means 10%)
        /// </summary>
        [Required]
        [Column(TypeName = "decimal(5,2)")]
        public decimal Percentage { get; set; }

        /// <summary>
        /// Maximum discount amount cap in rupees. 0 means no cap.
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal MaxDiscountAmount { get; set; }

        /// <summary>
        /// If true, this coupon is auto-applied on user's first policy purchase.
        /// </summary>
        public bool IsFirstTimeOnly { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime ValidFrom { get; set; } = DateTime.UtcNow;

        public DateTime? ValidUntil { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
