using System;
using System.Linq;
using System.Threading.Tasks;
using TechXpress.Data.Models;
using TechXpress.Data.Repositories.ShoppingCartRepo;
using TechXpress.Data.UnitOfWork;
using TechXpress.Services.CartItemsService;
using TechXpress.Services.GenericServices;
using TechXpress.Services.OrdersService;

namespace TechXpress.Services.ShoppingCartsService
{
    public class ShoppingCartService : GenericService<ShoppingCart>, IShoppingCartService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IShoppingCartRepository _shoppingCartRepository;
        private readonly ICartItemService _cartItemService;

        public ShoppingCartService(
            IUnitOfWork unitOfWork,
            ICartItemService cartItemService)
            : base(unitOfWork.ShoppingCarts)
        {
            _unitOfWork = unitOfWork;
            _shoppingCartRepository = unitOfWork.ShoppingCarts as IShoppingCartRepository;
            _cartItemService = cartItemService;
        }


        public async Task<string> Test()
        {
            var sampleCart = await _shoppingCartRepository.GetWithItemsAsync(1);
            return $"ShoppingCart Service operational. Sample cart has {sampleCart?.Items.Count ?? 0} items";
        }

        public async Task<ShoppingCart> GetCartByIdAsync(string cartId, bool includeItems = false)
        {
            return await _unitOfWork.ShoppingCarts.GetCartByIdAsync(cartId, includeItems);
        }

        public async Task<ShoppingCart> GetOrCreateUserCartAsync(string userId, bool includeItems = false)
        {
            var cart = await _unitOfWork.ShoppingCarts.GetUserCartAsync(userId, includeItems);
            if (cart == null)
            {
                cart = await _unitOfWork.ShoppingCarts.CreateCartAsync(userId);
                await _unitOfWork.CompleteAsync(); // Save changes
            }
            return cart;
        }

        public async Task MergeGuestCartWithUserCartAsync(string guestCartId, string userId)
        {
            if (!int.TryParse(guestCartId, out var guestCartIdAsInt))
            {
                throw new ArgumentException("guestCartId must be a valid integer string.", nameof(guestCartId));
            }

            await _unitOfWork.ShoppingCarts.MergeGuestCartWithUserCartAsync(userId, guestCartIdAsInt);
            await _unitOfWork.CompleteAsync();
        }

        public async Task<ShoppingCart> CreateGuestCartAsync()
        {
            var cart = await _unitOfWork.ShoppingCarts.CreateCartAsync();
            await _unitOfWork.CompleteAsync();
            return cart;
        }

        public async Task<ShoppingCart> GetCartByUserIdAsync(string userId)
        {
            return await _shoppingCartRepository.GetByUserIdAsync(userId);
        }

        public async Task<ShoppingCart> GetCartWithItemsAsync(int cartId)
        {
            return await _shoppingCartRepository.GetWithItemsAsync(cartId);
        }

        public async Task<decimal> CalculateCartTotalAsync(int cartId)
        {
            return await _cartItemService.CalculateCartTotalAsync(cartId);
        }

        public async Task ClearCartAsync(int cartId)
        {
            await _shoppingCartRepository.ClearCartAsync(cartId);
            await _unitOfWork.CompleteAsync();
        }

        public async Task<bool> CartExistsForUserAsync(string userId)
        {
            return await _shoppingCartRepository.CartExistsForUserAsync(userId);
        }

        public async Task AddItemToCartAsync(int cartId, int productId, int quantity = 1)
        {
            var existingItem = await _cartItemService.GetByProductAndCartAsync(productId, cartId);

            if (existingItem != null)
            {
                await _cartItemService.IncrementQuantityAsync(existingItem.Id, quantity);
            }
            else
            {
                var newItem = new CartItem
                {
                    ShoppingCartId = cartId,
                    ProductId = productId,
                    Quantity = quantity
                };
                await _cartItemService.AddAsync(newItem);
            }

            await _unitOfWork.CompleteAsync();
        }

        public async Task RemoveItemFromCartAsync(int cartId, int productId)
        {
            var item = await _cartItemService.GetByProductAndCartAsync(productId, cartId);
            if (item != null)
            {
                await _cartItemService.DeleteAsync(item);
                await _unitOfWork.CompleteAsync();
            }
        }

        public async Task UpdateItemQuantityAsync(int cartId, int productId, int newQuantity)
        {
            if (newQuantity <= 0)
            {
                await RemoveItemFromCartAsync(cartId, productId);
                return;
            }

            var item = await _cartItemService.GetByProductAndCartAsync(productId, cartId);
            if (item != null)
            {
                await _cartItemService.UpdateQuantityAsync(item.Id, newQuantity);
                await _unitOfWork.CompleteAsync();
            }
        }

        public async Task<Order> ConvertCartToOrderAsync(int cartId, AddressViewModel shippingAddress)
        {
            var cart = await GetCartWithItemsAsync(cartId);
            if (cart == null || !cart.Items.Any())
                throw new InvalidOperationException("Cart is empty or doesn't exist");

            var orderService = _unitOfWork.Orders as IOrderService;
            var orderDetails = cart.Items.Select(i => new OrderDetail
            {
                ProductId = i.ProductId,
                Quantity = i.Quantity,
                Price = i.Product.Price
            }).ToList();

            var order = await orderService.CreateOrderAsync(
                cart.UserId,
                shippingAddress,
                orderDetails);

            await ClearCartAsync(cartId);
            return order;
        }

        public async Task MergeCartsAsync(string userId, int temporaryCartId)
        {
            var userCart = await GetCartByUserIdAsync(userId) ??
                await CreateUserCartAsync(userId);

            var tempCart = await GetCartWithItemsAsync(temporaryCartId);

            foreach (var item in tempCart.Items)
            {
                await AddItemToCartAsync(userCart.Id, item.ProductId, item.Quantity);
            }

            await ClearCartAsync(temporaryCartId);
        }

        private async Task<ShoppingCart> CreateUserCartAsync(string userId)
        {
            var newCart = new ShoppingCart { UserId = userId };
            await _shoppingCartRepository.AddAsync(newCart);
            await _unitOfWork.CompleteAsync();
            return newCart;
        }

        public async Task<int> GetCartItemCountAsync(int cartId)
        {
            var cart = await GetCartWithItemsAsync(cartId);
            return cart?.Items?.Sum(item => item.Quantity) ?? 0;
        }
    }
}