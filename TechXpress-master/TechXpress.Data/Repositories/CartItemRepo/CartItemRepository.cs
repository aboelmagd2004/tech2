using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TechXpress.Data.Models;
using TechXpress.Data.Models.Contexts;
using TechXpress.Data.Repositories.GenericRepository;

namespace TechXpress.Data.Repositories.CartItemRepo
{
    public class CartItemRepository : GenericRepository<CartItem>, ICartItemRepository
    {
        public CartItemRepository(TechXpressDbContext context) : base(context)
        {
        }

        public async Task<CartItem> GetByProductAndCartAsync(int productId, int cartId)
        {
            return await _context.Set<CartItem>()
                .Include(i => i.Product)
                .FirstOrDefaultAsync(i => i.ProductId == productId && i.ShoppingCartId == cartId);
        }

        public async Task UpdateQuantityAsync(int itemId, int newQuantity)
        {
            var item = await _context.Set<CartItem>().FindAsync(itemId);
            if (item != null)
            {
                item.Quantity = newQuantity;
                _context.Set<CartItem>().Update(item);
            }
        }

        public async Task<IEnumerable<CartItem>> GetItemsForCartAsync(int cartId)
        {
            return await _context.Set<CartItem>()
                .Include(i => i.Product)
                .Where(i => i.ShoppingCartId == cartId)
                .ToListAsync();
        }

        public async Task<bool> ProductExistsInCartAsync(int productId, int cartId)
        {
            return await _context.Set<CartItem>()
                .AnyAsync(i => i.ProductId == productId && i.ShoppingCartId == cartId);
        }

        public async Task<CartItem> GetCartItemWithProductAsync(int cartItemId)
        {
            return await _context.Set<CartItem>()
                .Include(i => i.Product)
                .Include(i => i.ShoppingCart)
                .FirstOrDefaultAsync(i => i.Id == cartItemId);
        }

        public async Task<int> GetCartItemCountAsync(int cartId)
        {
            return await _context.Set<CartItem>()
                .Where(i => i.ShoppingCartId == cartId)
                .SumAsync(i => i.Quantity);
        }

        public async Task<decimal> CalculateCartTotalAsync(int cartId)
        {
            return await _context.Set<CartItem>()
                .Where(i => i.ShoppingCartId == cartId)
                .SumAsync(i => i.Product.Price * i.Quantity);
        }

        public async Task AddRangeAsync(IEnumerable<CartItem> cartItems)
        {
            await _context.Set<CartItem>().AddRangeAsync(cartItems);
        }

        public async Task RemoveRangeAsync(IEnumerable<CartItem> cartItems)
        {
            _context.Set<CartItem>().RemoveRange(cartItems);
        }

        public async Task<IEnumerable<CartItem>> GetFilteredCartItemsAsync(
            Expression<Func<CartItem, bool>> filter = null,
            Func<IQueryable<CartItem>, IOrderedQueryable<CartItem>> orderBy = null,
            string includeProperties = "",
            int? skip = null,
            int? take = null)
        {
            IQueryable<CartItem> query = _context.Set<CartItem>();

            if (filter != null)
            {
                query = query.Where(filter);
            }

            foreach (var includeProperty in includeProperties.Split(
                new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
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

            return await query.ToListAsync();
        }
    }
}