using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TechXpress.Data.Models;
using TechXpress.Data.Models.Contexts;
using TechXpress.Data.Repositories.GenericRepository;

namespace TechXpress.Data.Repositories.ShoppingCartRepo
{
    public class ShoppingCartRepository : GenericRepository<ShoppingCart>, IShoppingCartRepository
    {
        public ShoppingCartRepository(TechXpressDbContext context) : base(context)
        {
        }

        public async Task<ShoppingCart> GetByUserIdAsync(string userId)
        {
            return await _context.Set<ShoppingCart>()
                .Include(c => c.Items)
                .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId);
        }

        public async Task<ShoppingCart> GetCartByIdAsync(string cartId, bool includeItems = false)
        {
            var query = _context.ShoppingCarts.AsQueryable();

            if (includeItems)
            {
                query = query
                    .Include(c => c.Items)
                        .ThenInclude(i => i.Product);
            }
            int cartIdInt= int.Parse(cartId);

            return await query.FirstOrDefaultAsync(c => c.Id == cartIdInt);
        }

        public async Task<ShoppingCart> GetUserCartAsync(string userId, bool includeItems = false)
        {
            var query = _context.ShoppingCarts
                .Where(c => c.UserId == userId);

            if (includeItems)
            {
                query = query
                    .Include(c => c.Items)
                        .ThenInclude(i => i.Product);
            }

            return await query.FirstOrDefaultAsync();
        }


        public async Task<ShoppingCart> CreateCartAsync(string userId = null)
        {
            var cart = new ShoppingCart
            {
                UserId = userId,
            };

            await _context.ShoppingCarts.AddAsync(cart);
            return cart;
        }

        public async Task MergeGuestCartWithUserCartAsync( string userId, int guestCartId)
        {
            var guestCart = await _context.ShoppingCarts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.Id == guestCartId);

            if (guestCart == null) return;

            var userCart = await GetUserCartAsync(userId, includeItems: true);
            if (userCart == null)
            {
                userCart = new ShoppingCart
                {
                    UserId = userId,
                    Items = new List<CartItem>()
                };
                await _context.ShoppingCarts.AddAsync(userCart);
            }

            foreach (var guestItem in guestCart.Items)
            {
                var existingItem = userCart.Items.FirstOrDefault(i => i.ProductId == guestItem.ProductId);
                if (existingItem != null)
                {
                    existingItem.Quantity += guestItem.Quantity;
                }
                else
                {
                    userCart.Items.Add(new CartItem
                    {
                        ProductId = guestItem.ProductId,
                        Quantity = guestItem.Quantity,
                        ShoppingCartId = userCart.Id
                    });
                }
            }

            // Remove guest cart  
            _context.ShoppingCarts.Remove(guestCart);
        }

        public async Task<ShoppingCart> GetWithItemsAsync(int cartId)
        {
            return await _context.Set<ShoppingCart>()
                .Include(c => c.Items)
                .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(c => c.Id == cartId);
        }

        public async Task<decimal> CalculateTotalAsync(int cartId)
        {
            return await _context.Set<ShoppingCart>()
                .Where(c => c.Id == cartId)
                .SelectMany(c => c.Items)
                .SumAsync(i => i.Product.Price * i.Quantity);
        }

        public async Task ClearCartAsync(int cartId)
        {
            var cart = await _context.Set<ShoppingCart>()
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.Id == cartId);

            if (cart != null)
            {
                _context.Set<CartItem>().RemoveRange(cart.Items);
            }
        }

        public async Task<bool> CartExistsForUserAsync(string userId)
        {
            return await _context.Set<ShoppingCart>()
                .AnyAsync(c => c.UserId == userId);
        }

        public async Task<int> GetCartItemCountAsync(int cartId)
        {
            return await _context.Set<ShoppingCart>()
                .Where(c => c.Id == cartId)
                .SelectMany(c => c.Items)
                .SumAsync(i => i.Quantity);
        }

        public async Task MergeCartsAsync(int targetCartId, int sourceCartId)
        {
            var targetCart = await GetWithItemsAsync(targetCartId);
            var sourceCart = await GetWithItemsAsync(sourceCartId);

            if (targetCart == null || sourceCart == null)
                return;

            foreach (var sourceItem in sourceCart.Items)
            {
                var existingItem = targetCart.Items
                    .FirstOrDefault(i => i.ProductId == sourceItem.ProductId);

                if (existingItem != null)
                {
                    existingItem.Quantity += sourceItem.Quantity;
                }
                else
                {
                    targetCart.Items.Add(new CartItem
                    {
                        ProductId = sourceItem.ProductId,
                        Quantity = sourceItem.Quantity,
                        ShoppingCartId = targetCartId
                    });
                }
            }

            await ClearCartAsync(sourceCartId);
            _context.Set<ShoppingCart>().Update(targetCart);
        }

        public async Task<IEnumerable<ShoppingCart>> GetFilteredCartsAsync(
            Expression<Func<ShoppingCart, bool>> filter = null,
            Func<IQueryable<ShoppingCart>, IOrderedQueryable<ShoppingCart>> orderBy = null,
            string includeProperties = "",
            int? skip = null,
            int? take = null)
        {
            IQueryable<ShoppingCart> query = _context.Set<ShoppingCart>();

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