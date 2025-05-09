using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using TechXpress.Data.Models;
using TechXpress.Data.Models.Contexts;
using TechXpress.Services.GenericServices;

namespace TechXpress.Services.ProductsService
{
    public interface IProductService : IGenericService<ProductViewModel>
    {
        Task<string> Test();

        // Product-specific operations
        Task<IEnumerable<ProductViewModel>> GetFeaturedProductsAsync(int count);

        Task<IEnumerable<ProductViewModel>> GetAllProductsWithCategoriesAsync();

        Task<ProductViewModel> GetProductsWithCategoryAsync(int productId);


        Task<IEnumerable<ProductViewModel>> GetProductsByCategoryAsync(int categoryId);
        Task<IEnumerable<ProductViewModel>> SearchProductsAsync(string searchTerm);
        Task UpdateStockAsync(int productId, int quantityChange);
        Task<bool> IsProductInStockAsync(int productId);
        Task<decimal> GetProductPriceAsync(int productId);
        Task ApplyDiscountAsync(int productId, decimal discountPercentage);

        public void UpdateSpecifications(ProductViewModel product, ICollection<ProductSpecification> newSpecifications);

        public TechXpressDbContext GetDbContext();

        // Advanced filtering
        Task<IEnumerable<ProductViewModel>> GetFilteredProductsAsync(
            Expression<Func<ProductViewModel, bool>> filter = null,
            Func<IQueryable<ProductViewModel>, IOrderedQueryable<ProductViewModel>> orderBy = null,
            string includeProperties = "",
            int? skip = null,
            int? take = null);
    }
}