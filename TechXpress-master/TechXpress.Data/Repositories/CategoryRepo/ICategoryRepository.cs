using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using TechXpress.Data.Models;
using TechXpress.Data.Repositories.GenericRepository;

namespace TechXpress.Data.Repositories.CategoryRepo
{
    public interface ICategoryRepository : IGenericRepository<Category>
    {
        // Basic test method
        string TestRepository();

        // Core category operations
        Task<IEnumerable<Category>> GetAllWithProductsAsync();
        Task<Category> GetByIdWithProductsAsync(int id);
        Task<Category> GetBySlugAsync(string slug);
        Task<IEnumerable<Category>> GetActiveCategoriesAsync();
        Task<IEnumerable<Category>> GetInactiveCategoriesAsync();
        // Add to ICategoryRepository interface
        Task<IEnumerable<ProductViewModel>> GetProductsByCategoryAsync(int categoryId, ProductSortOrder sortOrder = ProductSortOrder.Newest);

        // Hierarchical operations
        Task<IEnumerable<Category>> GetParentCategoriesAsync(int categoryId);
        Task<IEnumerable<Category>> GetChildCategoriesAsync(int categoryId);
        Task<IEnumerable<Category>> GetRelatedCategoriesAsync(int categoryId);
        Task<IEnumerable<Category>> GetRootCategoriesAsync();
        Task<int> GetCategoryLevelAsync(int categoryId);
        Task UpdateCategoryHierarchyAsync(int categoryId, int? newParentId);

        // Product count operations
        Task<int> GetProductCountAsync(int categoryId);
        Task<Dictionary<int, int>> GetProductCountForAllCategoriesAsync();
        Task<int> GetActiveProductCountAsync(int categoryId);

        // Advanced filtering
        Task<IEnumerable<Category>> GetFilteredCategoriesAsync(
            Expression<Func<Category, bool>> filter = null,
            Func<IQueryable<Category>, IOrderedQueryable<Category>> orderBy = null,
            string includeProperties = "",
            int? skip = null,
            int? take = null);

        // Search operations
        Task<IEnumerable<Category>> SearchCategoriesAsync(string searchTerm);
        Task<IEnumerable<Category>> SearchActiveCategoriesAsync(string searchTerm);

        // Bulk operations
        //Task ToggleActiveStatusAsync(int categoryId, bool isActive);
        Task<bool> ToggleActiveStatusAsync(int categoryId, bool isActive);
        Task BulkUpdateActiveStatusAsync(IEnumerable<int> categoryIds, bool isActive);
        Task ReorderCategoriesAsync(IEnumerable<CategoryOrderUpdate> orderUpdates);

        // DataTables support
        Task<(IEnumerable<Category> Categories, int TotalCount, int FilteredCount)> GetCategoriesForDataTablesAsync(
            int start,
            int length,
            string searchValue,
            int sortColumnIndex,
            string sortDirection);

        // Statistics
        Task<CategoryStatistics> GetCategoryStatisticsAsync();
        Task<IEnumerable<Category>> GetCategoriesForSelectAsync();

        // SEO operations
        Task<bool> SlugExistsAsync(string slug, int? excludeCategoryId = null);
        Task<string> GenerateUniqueSlugAsync(string name, int? excludeCategoryId = null);

        // Image operations
        Task UpdateImageUrlAsync(int categoryId, string imageUrl);
        Task RemoveImageAsync(int categoryId);

        // Product relationship operations
        Task MoveProductsToCategoryAsync(int sourceCategoryId, int targetCategoryId);
        Task<bool> HasProductsAsync(int categoryId);

        //Reorder 
    }

    public class CategoryOrderUpdate
    {
        public int CategoryId { get; set; }
        public int NewDisplayOrder { get; set; }
    }
    public class CategoryOrderUpdateDto
    {
        public int CategoryId { get; set; }
        public int NewDisplayOrder { get; set; }
    }
    public class CategoryStatistics
    {
        public int TotalCategories { get; set; }
        public int ActiveCategories { get; set; }
        public int InactiveCategories { get; set; }
        public int CategoriesWithProducts { get; set; }
        public int EmptyCategories { get; set; }
    }

    public enum ProductSortOrder
    {
        Newest,
        PriceLowToHigh,
        PriceHighToLow,
        Name
    }


}