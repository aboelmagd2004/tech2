using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using TechXpress.Data.Models;
using X.PagedList;

namespace TechXpress.Web.ViewModels.Products
{
    public class ProductIndexViewModel
    {
        // Core listing data
        public IPagedList<Data.Models.ProductViewModel> Products { get; set; }
        public IEnumerable<Category> Categories { get; set; }
        public IEnumerable<Data.Models.ProductViewModel> RecommendedProducts { get; set; }

        // Filtering parameters
        public int? SelectedCategoryId { get; set; }
        public string SearchTerm { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public bool OnSale { get; set; }
        public bool InStock { get; set; }
        public bool IsFeatured { get; set; }

        // Sorting
        public string SortOrder { get; set; }
        public Dictionary<string, string> SortOptions { get; } = new()
        {
            { "", "Default" },
            { "name_asc", "Name (A-Z)" },
            { "name_desc", "Name (Z-A)" },
            { "price_asc", "Price (Low to High)" },
            { "price_desc", "Price (High to Low)" },
            { "newest", "Newest" },
            { "oldest", "Oldest" },
            { "rating", "Top Rated" },
            { "popular", "Most Popular" }
        };

        // Pagination
        public int PageSize { get; set; } = 12;
        public Dictionary<int, string> PageSizeOptions { get; } = new()
        {
            { 12, "12 per page" },
            { 24, "24 per page" },
            { 48, "48 per page" },
            { 96, "96 per page" }
        };

        // UI state
        public int CartItemCount { get; set; }
        public int TotalProductCount { get; set; }
        public int FilteredProductCount { get; set; }

        // Price range for UI slider
        public decimal MinAvailablePrice { get; set; }
        public decimal MaxAvailablePrice { get; set; }

        // Methods
        public string GetSortDisplayName(string sortValue)
        {
            return SortOptions.TryGetValue(sortValue, out var displayName)
                ? displayName
                : sortValue;
        }

        public bool IsSortOptionSelected(string sortValue)
        {
            return string.Equals(SortOrder, sortValue, StringComparison.OrdinalIgnoreCase);
        }

        public Expression<Func<Data.Models.ProductViewModel, bool>> BuildFilterExpression()
        {
            return p =>
                (SelectedCategoryId == null || p.CategoryId == SelectedCategoryId) &&
                (string.IsNullOrEmpty(SearchTerm) ||
                 p.Name.Contains(SearchTerm) ||
                 p.Description.Contains(SearchTerm) ||
                 p.Category.Name.Contains(SearchTerm)) &&
                (!MinPrice.HasValue || p.Price >= MinPrice.Value) &&
                (!MaxPrice.HasValue || p.Price <= MaxPrice.Value) &&
                //(!OnSale || p.DiscountPrice.HasValue) &&
                (!InStock || p.StockQuantity > 0) &&
                (!IsFeatured || p.IsFeatured);
        }

        public Func<IQueryable<Data.Models.ProductViewModel>, IOrderedQueryable<Data.Models.ProductViewModel>> BuildSortFunction()
        {
            return SortOrder switch
            {
                "name_asc" => q => q.OrderBy(p => p.Name),
                "name_desc" => q => q.OrderByDescending(p => p.Name),
                //"price_asc" => q => q.OrderBy(p => p.DiscountPrice ?? p.Price),
                //"price_desc" => q => q.OrderByDescending(p => p.DiscountPrice ?? p.Price),
                "newest" => q => q.OrderByDescending(p => p.CreatedDate),
                "oldest" => q => q.OrderBy(p => p.CreatedDate),
                "rating" => q => q.OrderByDescending(p => p.Reviews.Average(r => r.Rating)),
                "popular" => q => q.OrderByDescending(p => p.OrderDetails.Sum(od => od.Quantity)),
                _ => q => q.OrderBy(p => p.Name)
            };
        }
    }
}