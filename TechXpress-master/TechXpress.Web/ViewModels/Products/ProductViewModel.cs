using System;
using System.ComponentModel.DataAnnotations;

namespace TechXpress.Web.ViewModels.Products
{
    public class ProductViewModel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Price { get; set; }

        [Display(Name = "Current Price")]
        public decimal? CurrentPrice { get; set; }

        [Display(Name = "Original Price")]
        public decimal? OriginalPrice { get; set; }

        [Display(Name = "Image URL")]
        [Url]
        public string ImageUrl { get; set; }

        [Display(Name = "Additional Images")]
        public List<string> AdditionalImages { get; set; } = new List<string>();

        [Required]
        [StringLength(50)]
        public string SKU { get; set; }

        [Display(Name = "Stock Quantity")]
        public int StockQuantity { get; set; }

        [Display(Name = "Is Active")]
        public bool IsActive { get; set; }

        [Display(Name = "Category ID")]
        public int CategoryId { get; set; }

        [Display(Name = "Category Name")]
        public string CategoryName { get; set; }

        [Display(Name = "Created Date")]
        public DateTime CreatedDate { get; set; }

        [Display(Name = "Modified Date")]
        public DateTime ModifiedDate { get; set; }

        [Display(Name = "Average Rating")]
        [Range(0, 5)]
        public double? AverageRating { get; set; }

        [Display(Name = "Review Count")]
        public int ReviewCount { get; set; }

        [Display(Name = "Is Featured")]
        public bool IsFeatured { get; set; }

        [Display(Name = "Is On Sale")]
        public bool IsOnSale { get; set; }

        [Display(Name = "Discount Percentage")]
        [Range(0, 100)]
        public int? DiscountPercentage { get; set; }

        [Display(Name = "Manufacturer")]
        [StringLength(100)]
        public string Manufacturer { get; set; }

        [Display(Name = "Weight (kg)")]
        [Range(0, double.MaxValue)]
        public double? Weight { get; set; }

        [Display(Name = "Dimensions")]
        public string Dimensions { get; set; }

        [Display(Name = "Meta Title")]
        [StringLength(70)]
        public string MetaTitle { get; set; }

        [Display(Name = "Meta Description")]
        [StringLength(160)]
        public string MetaDescription { get; set; }

        [Display(Name = "Meta Keywords")]
        [StringLength(500)]
        public string MetaKeywords { get; set; }

        [Display(Name = "Short Description")]
        [StringLength(200)]
        public string ShortDescription { get; set; }

        // Helper properties
        public bool IsInStock => StockQuantity > 0;
        public bool HasDiscount => IsOnSale && DiscountPercentage.HasValue;
        public decimal DiscountedPrice => HasDiscount ? Price * (1 - (DiscountPercentage.Value / 100m)) : Price;
    }
}