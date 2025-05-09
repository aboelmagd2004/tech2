using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using TechXpress.Data.Models;
using TechXpress.Data.Repositories.GenericRepository;

namespace TechXpress.Data.Repositories.CartItemRepo
{
    public interface ICartItemRepository : IGenericRepository<CartItem>
    {
        Task<CartItem> GetByProductAndCartAsync(int productId, int cartId);
        Task UpdateQuantityAsync(int itemId, int newQuantity);
        Task<IEnumerable<CartItem>> GetItemsForCartAsync(int cartId);
        Task<bool> ProductExistsInCartAsync(int productId, int cartId);

        // Additional methods for complete functionality
        Task<CartItem> GetCartItemWithProductAsync(int cartItemId);
        Task<int> GetCartItemCountAsync(int cartId);
        Task<decimal> CalculateCartTotalAsync(int cartId);
        Task AddRangeAsync(IEnumerable<CartItem> cartItems);
        Task RemoveRangeAsync(IEnumerable<CartItem> cartItems);

        Task<IEnumerable<CartItem>> GetFilteredCartItemsAsync(
            Expression<Func<CartItem, bool>> filter = null,
            Func<IQueryable<CartItem>, IOrderedQueryable<CartItem>> orderBy = null,
            string includeProperties = "",
            int? skip = null,
            int? take = null);
    }
}