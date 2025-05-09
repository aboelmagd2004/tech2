using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using TechXpress.Data.Models;
using TechXpress.Data.Repositories.GenericRepository;

namespace TechXpress.Data.Repositories.ProductRepo
{
    public interface IProductRepository : IGenericRepository<ProductViewModel>
    {
        // Basic test method
        public string test();

        // Core product operations
        Task<IEnumerable<ProductViewModel>> GetProductsWithCategoryAsync();
        Task<ProductViewModel> GetProductWithCategoryAsync(int id);
        Task<IEnumerable<ProductViewModel>> GetFeaturedProductsAsync(int count);
        Task<IEnumerable<ProductViewModel>> GetProductsByCategoryAsync(int categoryId);
        Task<IEnumerable<ProductViewModel>> SearchProductsAsync(string searchTerm);
        Task UpdateStockAsync(int productId, int quantity);
        Task<IEnumerable<ProductViewModel>> GetProductsByIdsAsync(IEnumerable<int> productIds);

        // Advanced filtering
        Task<IEnumerable<ProductViewModel>> GetFilteredProductsAsync(
            Expression<Func<ProductViewModel, bool>> filter = null,
            Func<IQueryable<ProductViewModel>, IOrderedQueryable<ProductViewModel>> orderBy = null,
            string includeProperties = "",
            int? skip = null,
            int? take = null);

        // Inventory management
        Task<int> GetProductStockAsync(int productId);
        Task<bool> IsProductInStockAsync(int productId);

        // Price operations
        Task<decimal> GetProductPriceAsync(int productId);
        Task ApplyDiscountAsync(int productId, decimal discountPercentage);
    }
}