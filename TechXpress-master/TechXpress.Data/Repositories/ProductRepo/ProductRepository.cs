using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TechXpress.Data.Models;
using TechXpress.Data.Models.Contexts;
using TechXpress.Data.Repositories.GenericRepository;

namespace TechXpress.Data.Repositories.ProductRepo
{
    public class ProductRepository : GenericRepository<ProductViewModel>, IProductRepository
    {
        public ProductRepository(TechXpressDbContext context) : base(context)
        {
        }

        public string test()
        {
            return "Product Repository is operational";
        }

        public async Task<IEnumerable<ProductViewModel>> GetProductsWithCategoryAsync()
        {
            return await _context.Products
                .Include(p => p.Category)
                .Include(p=>p.Specifications)
                .Include(P=>P.Reviews)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<ProductViewModel> GetProductWithCategoryAsync(int id)
        {
            return await _context.Products
                .Include(p => p.Category)
                .Include(p=>p.Specifications)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<IEnumerable<ProductViewModel>> GetFeaturedProductsAsync(int count)
        {
            return await _context.Products
                .Where(p => p.IsFeatured)
                .OrderByDescending(p => p.CreatedDate)
                .Take(count)
                .Include(p => p.Category)
                .Include(p=>p.Reviews)
                .ToListAsync();
        }

        public async Task<IEnumerable<ProductViewModel>> GetProductsByCategoryAsync(int categoryId)
        {
            return await _context.Products
                .Where(p => p.CategoryId == categoryId)
                .Include(p => p.Category)
                .ToListAsync();
        }

        public async Task<IEnumerable<ProductViewModel>> SearchProductsAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return await GetProductsWithCategoryAsync();

            return await _context.Products
                .Where(p => p.Name.Contains(searchTerm) ||
                           p.Description.Contains(searchTerm) ||
                           p.Category.Name.Contains(searchTerm))
                .Include(p => p.Category)
                .ToListAsync();
        }

        public async Task UpdateStockAsync(int productId, int quantity)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product != null)
            {
                product.StockQuantity += quantity;
                _context.Products.Update(product);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<ProductViewModel>> GetProductsByIdsAsync(IEnumerable<int> productIds)
        {
            return await _context.Products
                .Where(p => productIds.Contains(p.Id))
                .Include(p => p.Category)
                .ToListAsync();
        }

        public async Task<IEnumerable<ProductViewModel>> GetFilteredProductsAsync(
            Expression<Func<ProductViewModel, bool>> filter = null,
            Func<IQueryable<ProductViewModel>, IOrderedQueryable<ProductViewModel>> orderBy = null,
            string includeProperties = "",
            int? skip = null,
            int? take = null)
        {
            IQueryable<ProductViewModel> query = _context.Products;

            if (filter != null)
            {
                query = query.Where(filter);
            }

            foreach (var includeProperty in includeProperties.Split
                (new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                query = query.Include(includeProperty);
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

        public async Task<int> GetProductStockAsync(int productId)
        {
            return await _context.Products
                .Where(p => p.Id == productId)
                .Select(p => p.StockQuantity)
                .FirstOrDefaultAsync();
        }

        public async Task<bool> IsProductInStockAsync(int productId)
        {
            return await _context.Products
                .AnyAsync(p => p.Id == productId && p.StockQuantity > 0);
        }

        public async Task<decimal> GetProductPriceAsync(int productId)
        {
            return await _context.Products
                .Where(p => p.Id == productId)
                .Select(p => p.Price)
                .FirstOrDefaultAsync();
        }

        public async Task ApplyDiscountAsync(int productId, decimal discountPercentage)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product != null)
            {
                product.Price *= (1 - discountPercentage / 100);
                _context.Products.Update(product);
                await _context.SaveChangesAsync();
            }
        }
    }
}