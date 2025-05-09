using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using TechXpress.Data.Models;
using TechXpress.Services.ShoppingCartsService; // Import your shopping cart service
using Microsoft.AspNetCore.Authorization;

namespace TechXpress.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ShippingManagementController : Controller
    {
        private readonly IShoppingCartService _shoppingCartService;

        // Constructor injection of the shopping cart service
        public ShippingManagementController(IShoppingCartService shoppingCartService)
        {
            _shoppingCartService = shoppingCartService;
        }

        // GET: Admin/ShippingManagement/Index
        public async Task<IActionResult> Index()
        {
            // Example: List all active carts (or perhaps those ready for shipping)
            var carts = await _shoppingCartService.GetAllAsync();
            return View(carts); // View will display the list of carts
        }

        // GET: Admin/ShippingManagement/Details/{cartId}
        public async Task<IActionResult> Details(int cartId)
        {
            var cart = await _shoppingCartService.GetCartWithItemsAsync(cartId);
            if (cart == null)
            {
                return NotFound();
            }
            return View(cart); // Display detailed information about the cart and shipping details
        }

        // GET: Admin/ShippingManagement/Ship/{cartId}
        public async Task<IActionResult> Ship(int cartId)
        {
            var cart = await _shoppingCartService.GetCartWithItemsAsync(cartId);
            if (cart == null)
            {
                return NotFound();
            }

            // Additional logic for preparing the cart for shipping (e.g., create shipping label, send notification, etc.)
            return View(cart); // Display a form or action for shipping the cart
        }

        // POST: Admin/ShippingManagement/Ship/{cartId}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ShipConfirmed(int cartId)
        {
            var cart = await _shoppingCartService.GetCartWithItemsAsync(cartId);
            if (cart == null)
            {
                return NotFound();
            }

            // Logic to confirm shipping the order (e.g., update order status)
            // For example, you can mark the order as "Shipped" or perform any other actions
            // The order conversion logic might be here as well

            await _shoppingCartService.ClearCartAsync(cartId); // Example: clearing cart after shipping
            return RedirectToAction(nameof(Index)); // Redirect to the cart list
        }

        // GET: Admin/ShippingManagement/ClearCart/{cartId}
        public async Task<IActionResult> ClearCart(int cartId)
        {
            await _shoppingCartService.ClearCartAsync(cartId); // Clear the cart contents
            return RedirectToAction(nameof(Index)); // Redirect to the cart list
        }
    }
}
