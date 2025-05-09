using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using TechXpress.Data.Models;
using TechXpress.Data.Repositories.CategoryRepo;
using TechXpress.Data.UnitOfWork;
using TechXpress.Services.GenericServices;

namespace TechXpress.Services.CategoriesService
{
    public class CategoryService : GenericService<Category>, ICategoryService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICategoryRepository _categoryRepository;

        public CategoryService(IUnitOfWork unitOfWork)
            : base(unitOfWork.Categories)
        {
            _unitOfWork = unitOfWork;
            _categoryRepository = unitOfWork.Categories as ICategoryRepository;
        }

        public async Task<string> Test()
        {
            var categories = await _categoryRepository.GetAllWithProductsAsync();
            return $"{categories.Count()} categories loaded | Category Service is operational";
        }

        // Basic CRUD operations (inherited from GenericService)
        // Add any custom CRUD operations here if needed

        // Category retrieval operations
        public async Task<IEnumerable<Category>> GetAllWithProductsAsync()
        {
            return await _categoryRepository.GetAllWithProductsAsync();
        }

        public async Task<Category> GetByIdWithProductsAsync(int id)
        {
            return await _categoryRepository.GetByIdWithProductsAsync(id);
        }
        public async Task<IEnumerable<ProductViewModel>> GetProductsByCategoryAsync(int categoryId, ProductSortOrder sortOrder = ProductSortOrder.Newest)
        {
            return await _categoryRepository.GetProductsByCategoryAsync(categoryId, sortOrder);
        }
        public async Task<Category> GetCategoryBySlugAsync(string slug)
        {
            return await _categoryRepository.GetBySlugAsync(slug);
        }

        public async Task<IEnumerable<Category>> GetAllActiveCategoriesAsync()
        {
            return await _categoryRepository.GetActiveCategoriesAsync();
        }

        public async Task<IEnumerable<Category>> GetAllCategoriesForSelectAsync()
        {
            return await _categoryRepository.GetCategoriesForSelectAsync();
        }

        // Hierarchical operations
        public async Task<IEnumerable<Category>> GetParentCategoriesAsync(int categoryId)
        {
            return await _categoryRepository.GetParentCategoriesAsync(categoryId);
        }

        public async Task<IEnumerable<Category>> GetChildCategoriesAsync(int categoryId)
        {
            return await _categoryRepository.GetChildCategoriesAsync(categoryId);
        }

        public async Task<IEnumerable<Category>> GetRelatedCategoriesAsync(int categoryId)
        {
            return await _categoryRepository.GetRelatedCategoriesAsync(categoryId);
        }

        public async Task UpdateCategoryHierarchyAsync(int categoryId, int? newParentId)
        {
            await _categoryRepository.UpdateCategoryHierarchyAsync(categoryId, newParentId);
            await _unitOfWork.CompleteAsync();
        }

        // Product relationship operations
        public async Task<int> GetProductCountAsync(int categoryId)
        {
            return await _categoryRepository.GetProductCountAsync(categoryId);
        }

        public async Task<int> GetActiveProductCountAsync(int categoryId)
        {
            return await _categoryRepository.GetActiveProductCountAsync(categoryId);
        }

        public async Task<Dictionary<int, int>> GetProductCountForAllCategoriesAsync()
        {
            return await _categoryRepository.GetProductCountForAllCategoriesAsync();
        }

        public async Task<bool> HasProductsAsync(int categoryId)
        {
            return await _categoryRepository.HasProductsAsync(categoryId);
        }

        public async Task MoveProductsToCategoryAsync(int sourceCategoryId, int targetCategoryId)
        {
            await _categoryRepository.MoveProductsToCategoryAsync(sourceCategoryId, targetCategoryId);
            await _unitOfWork.CompleteAsync();
        }

        // Filtering and searching
        public async Task<IEnumerable<Category>> GetFilteredCategoriesAsync(
            Expression<Func<Category, bool>> filter = null,
            Func<IQueryable<Category>, IOrderedQueryable<Category>> orderBy = null,
            string includeProperties = "",
            int? skip = null,
            int? take = null)
        {
            return await _categoryRepository.GetFilteredCategoriesAsync(
                filter, orderBy, includeProperties, skip, take);
        }

        public async Task<IEnumerable<Category>> SearchCategoriesAsync(string searchTerm)
        {
            return await _categoryRepository.SearchCategoriesAsync(searchTerm);
        }

        public async Task<IEnumerable<Category>> SearchActiveCategoriesAsync(string searchTerm)
        {
            return await _categoryRepository.SearchActiveCategoriesAsync(searchTerm);
        }

        // Status management
        public async Task<bool> ToggleCategoryActiveStatusAsync(int categoryId)
        {
            var category = await _categoryRepository.GetByIdAsync(categoryId);
            if (category == null)
                return false;

            bool newStatus = !category.IsActive;

            var result = await _categoryRepository.ToggleActiveStatusAsync(categoryId, newStatus);
            await _unitOfWork.CompleteAsync();
            return result;
        }

        public async Task BulkUpdateActiveStatusAsync(IEnumerable<int> categoryIds, bool isActive)
        {
            await _categoryRepository.BulkUpdateActiveStatusAsync(categoryIds, isActive);
            await _unitOfWork.CompleteAsync();
        }

        // Display order management
        //public async Task ReorderCategoriesAsync(IEnumerable<CategoryOrderUpdate> orderUpdates)
        //{
        //    await _categoryRepository.ReorderCategoriesAsync(orderUpdates);
        //    await _unitOfWork.CompleteAsync();
        //}

        // DataTables support
        public async Task<CategoryDataTablesResult> GetCategoriesForDataTablesAsync(
            int start,
            int length,
            string searchValue,
            int sortColumnIndex,
            string sortDirection)
        {
            var (data, totalRecords, filteredCount) = await _categoryRepository
                .GetCategoriesForDataTablesAsync(start, length, searchValue, sortColumnIndex, sortDirection);

            return new CategoryDataTablesResult
            {
                Data = data,
                TotalRecords = totalRecords,
                TotalRecordsFiltered = filteredCount
            };
        }

        // Statistics
        public async Task<CategoryStatistics> GetCategoryStatisticsAsync()
        {
            return await _categoryRepository.GetCategoryStatisticsAsync();
        }

        // SEO operations
        public async Task<bool> SlugExistsAsync(string slug, int? excludeCategoryId = null)
        {
            return await _categoryRepository.SlugExistsAsync(slug, excludeCategoryId);
        }

        public async Task<string> GenerateUniqueSlugAsync(string name, int? excludeCategoryId = null)
        {
            return await _categoryRepository.GenerateUniqueSlugAsync(name, excludeCategoryId);
        }

        // Image operations
        public async Task UpdateCategoryImageAsync(int categoryId, string imageUrl)
        {
            await _categoryRepository.UpdateImageUrlAsync(categoryId, imageUrl);
            await _unitOfWork.CompleteAsync();
        }

        public async Task RemoveCategoryImageAsync(int categoryId)
        {
            await _categoryRepository.RemoveImageAsync(categoryId);
            await _unitOfWork.CompleteAsync();
        }

        public async Task ReorderCategoriesAsync(IEnumerable<CategoryOrderUpdate> orderUpdates)
        {
            if (orderUpdates == null || !orderUpdates.Any())
            {
                throw new ArgumentException("No order updates provided");
            }

            try
            {
                await _categoryRepository.ReorderCategoriesAsync(orderUpdates);
                await _unitOfWork.CompleteAsync();

                // Clear cache since display order has changed
                //_cache.Remove(CategoriesCacheKey);
            }
            catch (Exception ex)
            {
                //_logger.LogError(ex, "Error reordering categories");
                throw; // Re-throw to be handled by controller
            }
        }

        // Product sorting and export
        //public async Task<IEnumerable<Product>> GetProductsByCategoryAsync(int categoryId, ProductSortOrder sortOrder = ProductSortOrder.Newest)
        //{
        //    return await _categoryRepository.GetProductsByCategoryAsync(categoryId, sortOrder);
        //}
    }

    // DTOs and Enums moved to separate files in a real project
  
}
