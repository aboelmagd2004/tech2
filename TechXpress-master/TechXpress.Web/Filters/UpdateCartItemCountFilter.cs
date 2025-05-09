using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;
using TechXpress.Services.ShoppingCartsService;

namespace TechXpress.Web.Filters
{
    public class UpdateCartItemCountFilter : IAsyncActionFilter
    {
        private readonly IShoppingCartService _cartService;

        public UpdateCartItemCountFilter(IShoppingCartService cartService)
        {
            _cartService = cartService;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (context.HttpContext.User.Identity?.IsAuthenticated == true)
            {
                var userId = context.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (!string.IsNullOrEmpty(userId))
                {
                    var cart = await _cartService.GetCartByUserIdAsync(userId);
                    if (cart != null)
                    {
                        var count = await _cartService.GetCartItemCountAsync(cart.Id);
                        context.HttpContext.Items["CartItemCount"] = count;
                    }
                }
            }

            await next();
        }
    }
}
