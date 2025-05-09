using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using TechXpress.Data.Models;
using TechXpress.Services.GenericServices;

namespace TechXpress.Services.CartItemsService
{
    public interface ICartItemService : IGenericService<CartItem>
    {
        Task<string> Test();

        // CartItem-specific operations
        Task<CartItem> GetCartItemWithProductAsync(int cartItemId);
        Task<IEnumerable<CartItem>> GetItemsForCartAsync(int cartId);
        Task<CartItem> GetByProductAndCartAsync(int productId, int cartId);

        // Quantity management
        Task UpdateQuantityAsync(int itemId, int newQuantity);
        Task IncrementQuantityAsync(int itemId, int increment = 1);
        Task DecrementQuantityAsync(int itemId, int decrement = 1);

        // Cart operations
        Task<bool> ProductExistsInCartAsync(int productId, int cartId);
        Task<int> GetCartItemCountAsync(int cartId);
        Task<decimal> CalculateCartTotalAsync(int cartId);

        // Item operations
        Task DeleteAsync(CartItem entity); // Added DeleteAsync
        Task DeleteAsync(int id); // Added overload for ID-based deletion

        // Batch operations
        Task AddRangeAsync(IEnumerable<CartItem> cartItems);
        Task RemoveRangeAsync(IEnumerable<CartItem> cartItems);

        // Advanced filtering
        Task<IEnumerable<CartItem>> GetFilteredCartItemsAsync(
            Expression<Func<CartItem, bool>> filter = null,
            Func<IQueryable<CartItem>, IOrderedQueryable<CartItem>> orderBy = null,
            string includeProperties = "",
            int? skip = null,
            int? take = null);
    }
}