using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace TechXpress.Data.Models
{
    public class Order
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Order Number")]
        public string OrderNumber { get; set; } = GenerateOrderNumber();

        [Required]
        [DataType(DataType.DateTime)]
        [Display(Name = "Order Date")]
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Total Amount")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public decimal TotalAmount { get; set; } = 0.01m;

        [Required]
        [Display(Name = "Payment Status")]
        public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;

        [Required]
        [Display(Name = "Shipping Address")]
        public AddressViewModel ShippingAddress { get; set; } = new AddressViewModel();

        [Required]
        [Display(Name = "Order Status")]
        public OrderStatus OrderStatus { get; set; } = OrderStatus.Pending;

        [StringLength(500)]
        [Display(Name = "Customer Notes")]
        public string CustomerNotes { get; set; } = string.Empty;

        [StringLength(500)]
        [Display(Name = "Admin Notes")]
        public string AdminNotes { get; set; } = string.Empty;

        [DataType(DataType.DateTime)]
        [Display(Name = "Last Updated")]
        public DateTime? LastUpdated { get; set; } = DateTime.UtcNow;

        // Foreign keys
        [Required]
        [Display(Name = "User")]
        public string? UserId { get; set; } = string.Empty;

        [ForeignKey("UserId")]
        public virtual ApplicationUser? User { get; set; } = new ApplicationUser();

        [InverseProperty("Order")]
        public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

        [InverseProperty("Order")]
        public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

        [NotMapped]
        [Display(Name = "Is Paid")]
        public bool IsPaid => Payments.Any(p => p.Status == PaymentStatus.Completed);

        [NotMapped]
        [Display(Name = "Item Count")]
        public int ItemCount => OrderDetails?.Sum(od => od.Quantity) ?? 0;

        private static string GenerateOrderNumber()
        {
            return $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";
        }
    }

    public enum OrderStatus
    {
        [Display(Name = "Pending")]
        Pending = 0,

        [Display(Name = "Processing")]
        Processing = 1,

        [Display(Name = "Shipped")]
        Shipped = 2,

        [Display(Name = "Delivered")]
        Delivered = 3,

        [Display(Name = "Cancelled")]
        Cancelled = 4,

        [Display(Name = "Returned")]
        Returned = 5,

        [Display(Name = "On Hold")]
        OnHold = 6,

        [Display(Name = "Refunded")]
        Refunded = 7
    }

    public class AddressViewModel
    {
        //primary Key 
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? AddressLine1 { get; set; }
        public string? AddressLine2 { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? ZipCode { get; set; }
        public string? Country { get; set; } = "US";
        public string? PhoneNumber { get; set; }
    }
}