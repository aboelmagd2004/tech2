using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TechXpress.Services;
using TechXpress.Services.ProductsService;
using TechXpress.Services.ShoppingCartsService;

namespace TechXpress.Web.Controllers
{
    public class BaseController : Controller
    {
        protected readonly IProductService _productService;
        protected readonly IShoppingCartService _cartService;

        public BaseController(
            IProductService productService,
            IShoppingCartService cartService)
        {
            _productService = productService;
            _cartService = cartService;
        }

        protected async Task<int> GetCartItemCountAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                // User is not signed in; return 0 items
                return 0;
            }

            var cart = await _cartService.GetCartByUserIdAsync(userId);

            if (cart == null)
            {
                // No cart found; return 0 items
                return 0;
            }

            return await _cartService.GetCartItemCountAsync(cart.Id);
        }

    }
}