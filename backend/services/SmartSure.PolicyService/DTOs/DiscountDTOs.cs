using System.ComponentModel.DataAnnotations;

namespace SmartSure.PolicyService.DTOs
{
    /// <summary>
    /// Represent or implements DiscountDTO.
    /// </summary>
    public class DiscountDTO
    {
        public Guid DiscountId { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public decimal Percentage { get; set; }
        public decimal MaxDiscountAmount { get; set; }
        public bool IsFirstTimeOnly { get; set; }
        public bool IsActive { get; set; }
        public DateTime ValidFrom { get; set; }
        public DateTime? ValidUntil { get; set; }
    }

    /// <summary>
    /// Represent or implements CreateDiscountDTO.
    /// </summary>
    public class CreateDiscountDTO
    {
        [Required(ErrorMessage = "Code is required")]
        [StringLength(50)]
        public string Code { get; set; }

        [Required(ErrorMessage = "Description is required")]
        [StringLength(200)]
        public string Description { get; set; }

        [Required]
        [Range(0.01, 100, ErrorMessage = "Percentage must be between 0.01 and 100")]
        public decimal Percentage { get; set; }

        public decimal MaxDiscountAmount { get; set; }

        public bool IsFirstTimeOnly { get; set; }

        public DateTime? ValidUntil { get; set; }
    }

    /// <summary>
    /// Represent or implements ApplyDiscountResultDTO.
    /// </summary>
    public class ApplyDiscountResultDTO
    {
        public decimal OriginalPremium { get; set; }
        public decimal DiscountPercentage { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal FinalPremium { get; set; }
        public string DiscountDescription { get; set; }
        public bool FirstTimeDiscount { get; set; }
        public string CouponCode { get; set; }
    }
}
