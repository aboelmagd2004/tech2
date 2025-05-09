using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TechXpress.Data.Models;
using TechXpress.Data.Models.Contexts;
using TechXpress.Data.Repositories.GenericRepository;

namespace TechXpress.Data.Repositories.CategoryRepo
{
    public class CategoryRepository : GenericRepository<Category>, ICategoryRepository
    {

        public CategoryRepository(TechXpressDbContext context) : base(context)
        {
        }

        public string TestRepository()
        {
            return "CategoryRepository is working!";
        }

        public async Task<IEnumerable<Category>> GetAllWithProductsAsync()
        {
            return await _context.Categories
                .Include(c => c.Products)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Category> GetByIdWithProductsAsync(int id)
        {
            return await _context.Categories
                .Include(c => c.Products)
                .Include(c=>c.ParentCategory)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<Category> GetBySlugAsync(string slug)
        {
            return await _context.Categories
                .FirstOrDefaultAsync(c => c.Slug == slug);
        }

        public async Task<IEnumerable<Category>> GetActiveCategoriesAsync()
        {
            return await _context.Categories
                .Where(c => c.IsActive)
                .OrderBy(c => c.DisplayOrder)
                .Select(c => new Category
                {
                    Id = c.Id,
                    Name = c.Name,
                    Slug = c.Slug,
                    Description = c.Description,
                    ImageUrl = c.ImageUrl,
                    DisplayOrder = c.DisplayOrder,
                    IsActive = c.IsActive,
                    ProductCount = _context.Products.Count(p => p.CategoryId == c.Id)
                })
                .AsNoTracking()
                .ToListAsync();
        }


        public async Task<IEnumerable<Category>> GetInactiveCategoriesAsync()
        {
            return await _context.Categories
                .Where(c => !c.IsActive)
                .OrderBy(c => c.DisplayOrder)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IEnumerable<Category>> GetParentCategoriesAsync(int categoryId)
        {
            var category = await _context.Categories
                .Include(c => c.ParentCategory)
                .FirstOrDefaultAsync(c => c.Id == categoryId);

            if (category == null || category.ParentId == null)
                return Enumerable.Empty<Category>();

            var parents = new List<Category>();
            var current = category.ParentCategory;

            while (current != null)
            {
                parents.Add(current);
                current = await _context.Categories
                    .Include(c => c.ParentCategory)
                    .FirstOrDefaultAsync(c => c.Id == current.ParentId);
            }

            return parents;
        }

        public async Task<IEnumerable<Category>> GetChildCategoriesAsync(int categoryId)
        {
            return await _context.Categories
                .Where(c => c.ParentId == categoryId)
                .OrderBy(c => c.DisplayOrder)
                .ToListAsync();
        }

        public async Task<IEnumerable<Category>> GetRelatedCategoriesAsync(int categoryId)
        {
            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.Id == categoryId);

            if (category == null)
                return Enumerable.Empty<Category>();

            return await _context.Categories
                .Where(c => c.ParentId == category.ParentId && c.Id != categoryId)
                .OrderBy(c => c.DisplayOrder)
                .Take(5)
                .ToListAsync();
        }

        public async Task<IEnumerable<Category>> GetRootCategoriesAsync()
        {
            return await _context.Categories
                .Where(c => c.ParentId == null)
                .OrderBy(c => c.DisplayOrder)
                .ToListAsync();
        }

        public async Task<int> GetCategoryLevelAsync(int categoryId)
        {
            var level = 0;
            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.Id == categoryId);

            while (category != null && category.ParentId != null)
            {
                level++;
                category = await _context.Categories
                    .FirstOrDefaultAsync(c => c.Id == category.ParentId);
            }

            return level;
        }

        public async Task UpdateCategoryHierarchyAsync(int categoryId, int? newParentId)
        {
            var category = await _context.Categories.FindAsync(categoryId);
            if (category == null)
                throw new ArgumentException("Category not found");

            category.ParentId = newParentId;
            category.Level = await GetCategoryLevelAsync(categoryId);
            category.ModifiedDate = DateTime.UtcNow;

            await UpdateChildrenLevels(categoryId, category.Level + 1);
            await _context.SaveChangesAsync();
        }

        private async Task UpdateChildrenLevels(int parentId, int level)
        {
            var children = await _context.Categories
                .Where(c => c.ParentId == parentId)
                .ToListAsync();

            foreach (var child in children)
            {
                child.Level = level;
                await UpdateChildrenLevels(child.Id, level + 1);
            }
        }

        public async Task<int> GetProductCountAsync(int categoryId)
        {
            return await _context.Products
                .CountAsync(p => p.CategoryId == categoryId);
        }

        public async Task<Dictionary<int, int>> GetProductCountForAllCategoriesAsync()
        {
            return await _context.Categories
                .GroupBy(c => c.Id)
                .Select(g => new { CategoryId = g.Key, Count = g.SelectMany(c => c.Products).Count() })
                .ToDictionaryAsync(x => x.CategoryId, x => x.Count);
        }

        public async Task<int> GetActiveProductCountAsync(int categoryId)
        {
            return await _context.Products
                .CountAsync(p => p.CategoryId == categoryId && p.IsActive);
        }

        public async Task<IEnumerable<Category>> GetFilteredCategoriesAsync(
            Expression<Func<Category, bool>> filter = null,
            Func<IQueryable<Category>, IOrderedQueryable<Category>> orderBy = null,
            string includeProperties = "",
            int? skip = null,
            int? take = null)
        {
            IQueryable<Category> query = _context.Categories;

            if (filter != null)
            {
                query = query.Where(filter);
            }

            foreach (var includeProperty in includeProperties.Split
                (new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                query = query.Include(includeProperty.Trim());
            }

            if (orderBy != null)
            {
                query = orderBy(query);
            }

            if (skip.HasValue)
            {
                query = query.Skip(skip.Value);
            }

            if (take.HasValue)
            {
                query = query.Take(take.Value);
            }

            return await query.AsNoTracking().ToListAsync();
        }

        public async Task<IEnumerable<Category>> SearchCategoriesAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return Enumerable.Empty<Category>();

            return await _context.Categories
                .Where(c => c.Name.Contains(searchTerm) ||
                           c.Description.Contains(searchTerm))
                .OrderBy(c => c.Name)
                .Take(10)
                .ToListAsync();
        }

        public async Task<IEnumerable<Category>> SearchActiveCategoriesAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return Enumerable.Empty<Category>();

            return await _context.Categories
                .Where(c => c.IsActive &&
                           (c.Name.Contains(searchTerm) ||
                           c.Description.Contains(searchTerm)))
                .OrderBy(c => c.Name)
                .Take(10)
                .ToListAsync();
        }

        public async Task<bool> ToggleActiveStatusAsync(int categoryId, bool isActive)
        {
            var category = await _context.Categories.FindAsync(categoryId);
            if (category == null)
            {
                return false;
            }

            category.IsActive = isActive;
            category.ModifiedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task BulkUpdateActiveStatusAsync(IEnumerable<int> categoryIds, bool isActive)
        {
            var categories = await _context.Categories
                .Where(c => categoryIds.Contains(c.Id))
                .ToListAsync();

            foreach (var category in categories)
            {
                category.IsActive = isActive;
                category.ModifiedDate = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
        }

        public async Task ReorderCategoriesAsync(IEnumerable<CategoryOrderUpdate> orderUpdates)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                foreach (var update in orderUpdates)
                {
                    var category = await _context.Categories.FindAsync(update.CategoryId);
                    if (category != null)
                    {
                        category.DisplayOrder = update.NewDisplayOrder;
                        category.ModifiedDate = DateTime.UtcNow;
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<(IEnumerable<Category> Categories, int TotalCount, int FilteredCount)>
            GetCategoriesForDataTablesAsync(
                int start,
                int length,
                string searchValue,
                int sortColumnIndex,
                string sortDirection)
        {
            var query = _context.Categories.AsQueryable();

            // Apply search filter
            if (!string.IsNullOrEmpty(searchValue))
            {
                searchValue = searchValue.ToLower();
                query = query.Where(c =>
                    c.Name.ToLower().Contains(searchValue) ||
                    c.Description.ToLower().Contains(searchValue) ||
                    c.Slug.ToLower().Contains(searchValue));
            }

            var totalCount = await query.CountAsync();

            // Apply sorting
            query = sortColumnIndex switch
            {
                0 => sortDirection == "asc"
                    ? query.OrderBy(c => c.Id)
                    : query.OrderByDescending(c => c.Id),
                1 => sortDirection == "asc"
                    ? query.OrderBy(c => c.Name)
                    : query.OrderByDescending(c => c.Name),
                2 => sortDirection == "asc"
                    ? query.OrderBy(c => c.Slug)
                    : query.OrderByDescending(c => c.Slug),
                3 => sortDirection == "asc"
                    ? query.OrderBy(c => c.DisplayOrder)
                    : query.OrderByDescending(c => c.DisplayOrder),
                4 => sortDirection == "asc"
                    ? query.OrderBy(c => c.IsActive)
                    : query.OrderByDescending(c => c.IsActive),
                _ => query.OrderBy(c => c.DisplayOrder)
            };

            var filteredCount = await query.CountAsync();

            // Apply pagination
            var categories = await query
                .Skip(start)
                .Take(length)
                .AsNoTracking()
                .ToListAsync();

            return (categories, totalCount, filteredCount);
        }

        public async Task<CategoryStatistics> GetCategoryStatisticsAsync()
        {
            var totalCategories = await _context.Categories.CountAsync();
            var activeCategories = await _context.Categories.CountAsync(c => c.IsActive);
            var inactiveCategories = totalCategories - activeCategories;

            var categoriesWithProducts = await _context.Categories
                .CountAsync(c => c.Products.Any());

            return new CategoryStatistics
            {
                TotalCategories = totalCategories,
                ActiveCategories = activeCategories,
                InactiveCategories = inactiveCategories,
                CategoriesWithProducts = categoriesWithProducts,
                EmptyCategories = totalCategories - categoriesWithProducts
            };
        }

        public async Task<IEnumerable<Category>> GetCategoriesForSelectAsync()
        {
            return await _context.Categories
                .OrderBy(c => c.Name)
                .Select(c => new Category
                {
                    Id = c.Id,
                    Name = c.Name,
                    ParentId = c.ParentId,
                    Level = c.Level
                })
                .ToListAsync();
        }

        public async Task<bool> SlugExistsAsync(string slug, int? excludeCategoryId = null)
        {
            return excludeCategoryId.HasValue
                ? await _context.Categories.AnyAsync(c => c.Slug == slug && c.Id != excludeCategoryId)
                : await _context.Categories.AnyAsync(c => c.Slug == slug);
        }

        public async Task<string> GenerateUniqueSlugAsync(string name, int? excludeCategoryId = null)
        {
            var slug = name.ToLower().Replace(" ", "-");
            var baseSlug = slug;
            var counter = 1;

            while (await SlugExistsAsync(slug, excludeCategoryId))
            {
                slug = $"{baseSlug}-{counter}";
                counter++;
            }

            return slug;
        }

        public async Task UpdateImageUrlAsync(int categoryId, string imageUrl)
        {
            var category = await _context.Categories.FindAsync(categoryId);
            if (category != null)
            {
                category.ImageUrl = imageUrl;
                category.ModifiedDate = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        public async Task RemoveImageAsync(int categoryId)
        {
            var category = await _context.Categories.FindAsync(categoryId);
            if (category != null)
            {
                category.ImageUrl = null;
                category.ModifiedDate = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        public async Task MoveProductsToCategoryAsync(int sourceCategoryId, int targetCategoryId)
        {
            var products = await _context.Products
                .Where(p => p.CategoryId == sourceCategoryId)
                .ToListAsync();

            foreach (var product in products)
            {
                product.CategoryId = targetCategoryId;
            }

            await _context.SaveChangesAsync();
        }
        public async Task<IEnumerable<ProductViewModel>> GetProductsByCategoryAsync(int categoryId, ProductSortOrder sortOrder = ProductSortOrder.Newest)
        {
            var query = _context.Products
                .Where(p => p.CategoryId == categoryId && p.IsActive)
                .AsQueryable();

            query = sortOrder switch
            {
                ProductSortOrder.PriceLowToHigh => query.OrderBy(p => p.Price),
                ProductSortOrder.PriceHighToLow => query.OrderByDescending(p => p.Price),
                ProductSortOrder.Name => query.OrderBy(p => p.Name),
                _ => query.OrderByDescending(p => p.CreatedDate) // Newest first
            };

            return await query
                .Include(p => p.Category)
                .AsNoTracking()
                .ToListAsync();
        }
        public async Task<bool> HasProductsAsync(int categoryId)
        {
            return await _context.Products
                .AnyAsync(p => p.CategoryId == categoryId);
        }

        public async Task ReorderCategoriesAsync(IEnumerable<CategoryOrderUpdateDto> orderUpdates)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Create dictionary for quick lookup
                var updateDict = orderUpdates.ToDictionary(u => u.CategoryId, u => u.NewDisplayOrder);

                // Get all affected categories in one query
                var categoryIds = updateDict.Keys.ToList();
                var categories = await _context.Categories
                    .Where(c => categoryIds.Contains(c.Id))
                    .ToListAsync();

                // Update display orders
                foreach (var category in categories)
                {
                    if (updateDict.TryGetValue(category.Id, out var newOrder))
                    {
                        category.DisplayOrder = newOrder;
                        category.ModifiedDate = DateTime.UtcNow;
                    }
                }

                // Save changes and commit transaction
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                //_logger.LogError(ex, "Error reordering categories");
                throw; // Re-throw to be handled by service layer
            }
        }
    }
}