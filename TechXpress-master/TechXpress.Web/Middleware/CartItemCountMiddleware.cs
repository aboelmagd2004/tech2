using System.Security.Claims;
using TechXpress.Services.ShoppingCartsService;

public class CartItemCountMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IServiceScopeFactory _scopeFactory;

    public CartItemCountMiddleware(RequestDelegate next, IServiceScopeFactory scopeFactory)
    {
        _next = next;
        _scopeFactory = scopeFactory;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            using var scope = _scopeFactory.CreateScope();
            var cartService = scope.ServiceProvider.GetRequiredService<IShoppingCartService>();

            var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(userId))
            {
                var cart = await cartService.GetCartByUserIdAsync(userId);
                if (cart != null)
                {
                    var count = cart.Items.Sum(item => item.Quantity);
                    context.Items["CartItemCount"] = count;
                }
            }
        }

        await _next(context);
    }
}
