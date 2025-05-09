using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using TechXpress.Data.Models;
using TechXpress.Data.Repositories.CategoryRepo;
using TechXpress.Services.GenericServices;

namespace TechXpress.Services.CategoriesService
{
    public interface ICategoryService : IGenericService<Category>
    {
        Task<string> Test();

        // Basic category operations
        Task<IEnumerable<Category>> GetAllWithProductsAsync();
        Task<Category> GetByIdWithProductsAsync(int id);
        Task<Category> GetCategoryBySlugAsync(string slug);
        Task<IEnumerable<Category>> GetAllActiveCategoriesAsync();
        Task<IEnumerable<Category>> GetAllCategoriesForSelectAsync();

        // Add to ICategoryService interface
        Task<IEnumerable<ProductViewModel>> GetProductsByCategoryAsync(int categoryId, ProductSortOrder sortOrder = ProductSortOrder.Newest);

        // Hierarchical operations
        Task<IEnumerable<Category>> GetParentCategoriesAsync(int categoryId);
        Task<IEnumerable<Category>> GetChildCategoriesAsync(int categoryId);
        Task<IEnumerable<Category>> GetRelatedCategoriesAsync(int categoryId);
        Task UpdateCategoryHierarchyAsync(int categoryId, int? newParentId);

        // Product operations
        Task<int> GetProductCountAsync(int categoryId);
        Task<int> GetActiveProductCountAsync(int categoryId);
        Task<Dictionary<int, int>> GetProductCountForAllCategoriesAsync();
        Task<bool> HasProductsAsync(int categoryId);
        Task MoveProductsToCategoryAsync(int sourceCategoryId, int targetCategoryId);

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

        // Status management
        Task<bool> ToggleCategoryActiveStatusAsync(int categoryId);
        Task BulkUpdateActiveStatusAsync(IEnumerable<int> categoryIds, bool isActive);

        // Display order management
        Task ReorderCategoriesAsync(IEnumerable<CategoryOrderUpdate> orderUpdates);
        // DataTables support
        Task<CategoryDataTablesResult> GetCategoriesForDataTablesAsync(
            int start,
            int length,
            string searchValue,
            int sortColumnIndex,
            string sortDirection);

        // Statistics
        Task<CategoryStatistics> GetCategoryStatisticsAsync();

        // SEO operations
        Task<bool> SlugExistsAsync(string slug, int? excludeCategoryId = null);
        Task<string> GenerateUniqueSlugAsync(string name, int? excludeCategoryId = null);

        // Image operations
        Task UpdateCategoryImageAsync(int categoryId, string imageUrl);
        Task RemoveCategoryImageAsync(int categoryId);

        // Export operations
    }

   

    public class CategoryDataTablesResult
    {
        public IEnumerable<Category> Data { get; set; }
        public int TotalRecords { get; set; }
        public int TotalRecordsFiltered { get; set; }
    }


    
}