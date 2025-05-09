using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using TechXpress.Data.Models;
using TechXpress.Services.GenericServices; // Import your generic service interface
using TechXpress.Services.ShoppingCartsService; // Import your shopping cart service
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace TechXpress.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class CustomerManagementController : Controller
    {
        private readonly IGenericService<ApplicationUser> _userService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IShoppingCartService _shoppingCartService;

        // Constructor injection of the services
        public CustomerManagementController(IGenericService<ApplicationUser> userService,
            UserManager<ApplicationUser> userManager,
            IShoppingCartService shoppingCartService)
        {
            _userService = userService;
            _shoppingCartService = shoppingCartService;
            _userManager = userManager;
        }

        // GET: Admin/CustomerManagement/Index
        public async Task<IActionResult> Index()
        {
            var users = await _userService.GetAllAsync(); // Assuming you have a generic service to get all users
            return View(users); // View will display the list of customers
        }

        // GET: Admin/CustomerManagement/Create
        public IActionResult Create()
        {
            return View(new ApplicationUser());
        }

        // POST: Admin/CustomerManagement/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ApplicationUser user)
        {
            ModelState.Remove("UserId");
            ModelState.Remove("User");

            if (ModelState.IsValid)
            {
                var result = await _userManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    return RedirectToAction(nameof(Index));
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            return View(user);
        }

        // GET: Admin/CustomerManagement/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var user = await _userService.GetByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // POST: Admin/CustomerManagement/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, ApplicationUser user)
        {
            ModelState.Remove("UserId");
            ModelState.Remove("User");
            if (id != user.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var existingUser = await _userService.GetByIdAsync(id);
                if (existingUser == null)
                {
                    return NotFound();
                }

                // Update user properties
                existingUser.FirstName = user.FirstName;
                existingUser.LastName = user.LastName;
                existingUser.Email = user.Email;
                existingUser.PhoneNumber = user.PhoneNumber;
                existingUser.AddressLine1 = user.AddressLine1;
                existingUser.AddressLine2 = user.AddressLine2;
                existingUser.City = user.City;
                existingUser.State = user.State;
                existingUser.PostalCode = user.PostalCode;
                existingUser.Country = user.Country;

                await _userService.UpdateAsync(existingUser);
                return RedirectToAction(nameof(Index));
            }
            return View(user);
        }

        // GET: Admin/CustomerManagement/Details/{userId}
        public async Task<IActionResult> Details(string Id)
        {
            var user = await _userService.GetByIdAsync(Id);

            if (user == null)
            {
                return NotFound();
            }
            return View(user); // Display detailed information about the user
        }

        // GET: Admin/CustomerManagement/Cart/{userId}
        public async Task<IActionResult> Cart(string Id)
        {
            var cart = await _shoppingCartService.GetCartByUserIdAsync(Id);
            if (cart == null)
            {
                return NotFound();
            }
            return View(cart); // Display the user's shopping cart
        }

        // GET: Admin/CustomerManagement/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var user = await _userService.GetByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // POST: Admin/CustomerManagement/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var user = await _userService.GetByIdAsync(id);
            if (user != null)
            {
                await _userService.DeleteAsync(user);
            }
            return RedirectToAction(nameof(Index));
        }
        // GET: Admin/CustomerManagement/Manage/{userId}
        public async Task<IActionResult> Manage(string Id)
        {
            var user = await _userService.GetByIdAsync(Id);
            if (user == null)
            {
                return NotFound();
            }
            return View(user); // Show user management options, such as updating details
        }

    }
}
