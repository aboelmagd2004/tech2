using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using TechXpress.Data.Models;
using TechXpress.Data.Repositories.GenericRepository;

namespace TechXpress.Data.Repositories.ShoppingCartRepo
{
    public interface IShoppingCartRepository : IGenericRepository<ShoppingCart>
    {
        Task<ShoppingCart> GetByUserIdAsync(string userId);
        Task<ShoppingCart> GetWithItemsAsync(int cartId);
        Task<decimal> CalculateTotalAsync(int cartId);
        Task ClearCartAsync(int cartId);
        Task<bool> CartExistsForUserAsync(string userId);

        //GetCartByIdAsync
        Task<ShoppingCart> GetCartByIdAsync(string cartId, bool includeItems = false);

        //GetUserCartAsync
        Task<ShoppingCart> GetUserCartAsync(string userId, bool includeItems = false);

        //CreateCartAsync
        Task<ShoppingCart> CreateCartAsync(string userId = null);

        //MergeGuestCartWithUserCartAsync
        Task MergeGuestCartWithUserCartAsync(string userId, int guestCartId);

        // Additional methods for complete functionality
        Task<int> GetCartItemCountAsync(int cartId);
        Task MergeCartsAsync(int targetCartId, int sourceCartId);

        Task<IEnumerable<ShoppingCart>> GetFilteredCartsAsync(
            Expression<Func<ShoppingCart, bool>> filter = null,
            Func<IQueryable<ShoppingCart>, IOrderedQueryable<ShoppingCart>> orderBy = null,
            string includeProperties = "",
            int? skip = null,
            int? take = null);
    }
}