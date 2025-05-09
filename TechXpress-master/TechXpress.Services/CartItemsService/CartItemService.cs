using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using TechXpress.Data.Models;
using TechXpress.Data.Repositories.CartItemRepo;
using TechXpress.Data.UnitOfWork;
using TechXpress.Services.GenericServices;

namespace TechXpress.Services.CartItemsService
{
    public class CartItemService : GenericService<CartItem>, ICartItemService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICartItemRepository _cartItemRepository;

        public CartItemService(IUnitOfWork unitOfWork)
            : base(unitOfWork.CartItems)
        {
            _unitOfWork = unitOfWork;
            _cartItemRepository = unitOfWork.CartItems as ICartItemRepository;
        }

        public async Task<string> Test()
        {
            var sampleItems = await _cartItemRepository.GetFilteredCartItemsAsync(take: 3);
            return $"CartItem Service operational. Sample cart ID: {sampleItems.FirstOrDefault()?.ShoppingCartId}";
        }

        public async Task<CartItem> GetCartItemWithProductAsync(int cartItemId)
        {
            return await _cartItemRepository.GetCartItemWithProductAsync(cartItemId);
        }

        public async Task<IEnumerable<CartItem>> GetItemsForCartAsync(int cartId)
        {
            return await _cartItemRepository.GetItemsForCartAsync(cartId);
        }

        public async Task<CartItem> GetByProductAndCartAsync(int productId, int cartId)
        {
            return await _cartItemRepository.GetByProductAndCartAsync(productId, cartId);
        }

        public async Task UpdateQuantityAsync(int itemId, int newQuantity)
        {
            if (newQuantity <= 0)
                throw new ArgumentException("Quantity must be greater than zero");

            await _cartItemRepository.UpdateQuantityAsync(itemId, newQuantity);
            await _unitOfWork.CompleteAsync();
        }

        public async Task IncrementQuantityAsync(int itemId, int increment = 1)
        {
            if (increment <= 0)
                throw new ArgumentException("Increment value must be positive");

            var item = await _cartItemRepository.GetByIdAsync(itemId);
            if (item != null)
            {
                item.Quantity += increment;
                _cartItemRepository.Update(item);
                await _unitOfWork.CompleteAsync();
            }
        }

        public async Task DecrementQuantityAsync(int itemId, int decrement = 1)
        {
            if (decrement <= 0)
                throw new ArgumentException("Decrement value must be positive");

            var item = await _cartItemRepository.GetByIdAsync(itemId);
            if (item != null)
            {
                item.Quantity = Math.Max(1, item.Quantity - decrement);
                _cartItemRepository.Update(item);
                await _unitOfWork.CompleteAsync();
            }
        }

        public async Task<bool> ProductExistsInCartAsync(int productId, int cartId)
        {
            return await _cartItemRepository.ProductExistsInCartAsync(productId, cartId);
        }

        public async Task<int> GetCartItemCountAsync(int cartId)
        {
            return await _cartItemRepository.GetCartItemCountAsync(cartId);
        }

        public async Task<decimal> CalculateCartTotalAsync(int cartId)
        {
            return await _cartItemRepository.CalculateCartTotalAsync(cartId);
        }

        public async Task AddRangeAsync(IEnumerable<CartItem> cartItems)
        {
            if (cartItems == null || !cartItems.Any())
                throw new ArgumentException("No cart items provided");

            await _cartItemRepository.AddRangeAsync(cartItems);
            await _unitOfWork.CompleteAsync();
        }

        public async Task RemoveRangeAsync(IEnumerable<CartItem> cartItems)
        {
            if (cartItems == null || !cartItems.Any())
                throw new ArgumentException("No cart items provided");

            _cartItemRepository.RemoveRangeAsync(cartItems);
            await _unitOfWork.CompleteAsync();
        }

        public async Task<IEnumerable<CartItem>> GetFilteredCartItemsAsync(
            Expression<Func<CartItem, bool>> filter = null,
            Func<IQueryable<CartItem>, IOrderedQueryable<CartItem>> orderBy = null,
            string includeProperties = "",
            int? skip = null,
            int? take = null)
        {
            return await _cartItemRepository.GetFilteredCartItemsAsync(
                filter,
                orderBy,
                includeProperties,
                skip,
                take);
        }

        public new async Task DeleteAsync(CartItem entity)
        {
            await base.DeleteAsync(entity);
            await _unitOfWork.CompleteAsync();
        }

        // Additional overload for convenience
        public async Task DeleteAsync(int id)
        {
            var entity = await GetByIdAsync(id);
            if (entity != null)
            {
                await DeleteAsync(entity);
            }
        }
    }
}