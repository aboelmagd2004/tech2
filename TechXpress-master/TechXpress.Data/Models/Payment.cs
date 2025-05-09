using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TechXpress.Data.Models
{
    public class Payment
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int OrderId { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public decimal Amount { get; set; }

        [Required]
        public PaymentMethod Method { get; set; }

        [Required]
        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

        [Required]
        [StringLength(50)]
        public string TransactionId { get; set; }

        [StringLength(1000)]
        public string? Notes { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime PaymentDate { get; set; } = DateTime.UtcNow;

        [DataType(DataType.DateTime)]
        public DateTime? ProcessedDate { get; set; }

        [StringLength(100)]
        public string? ProcessorResponse { get; set; }

        [StringLength(50)]
        public string? AuthorizationCode { get; set; }

        [StringLength(50)]
        public string? LastFourDigits { get; set; }

        // Navigation properties
        [ForeignKey("OrderId")]
        public virtual Order Order { get; set; }

        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }
    }

    public enum PaymentMethod
    {
        [Display(Name = "Credit Card")]
        CreditCard = 0,

        [Display(Name = "PayPal")]
        PayPal = 1,

        [Display(Name = "Bank Transfer")]
        BankTransfer = 2,

        [Display(Name = "Cash on Delivery")]
        CashOnDelivery = 3,

        [Display(Name = "Mobile Money")]
        MobileMoney = 4,

        [Display(Name = "Cryptocurrency")]
        Cryptocurrency = 5
    }

    public enum PaymentStatus
    {
        [Display(Name = "Pending")]
        Pending = 0,

        [Display(Name = "Authorized")]
        Authorized = 1,

        [Display(Name = "Completed")]
        Completed = 2,

        [Display(Name = "Failed")]
        Failed = 3,

        [Display(Name = "Refunded")]
        Refunded = 4,

        [Display(Name = "Partially Refunded")]
        PartiallyRefunded = 5,

        [Display(Name = "Voided")]
        Voided = 6,

        [Display(Name = "Disputed")]
        Disputed = 7
    }
}