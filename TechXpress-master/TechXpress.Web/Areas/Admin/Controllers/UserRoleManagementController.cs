using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TechXpress.Data.Models;
using TechXpress.Web.Areas.Admin.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TechXpress.Web.Areas.Admin.Models.Role;

namespace TechXpress.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class UserRoleManagementController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IAdminDashboardService _adminDashboardService;
        private readonly ILogger<UserRoleManagementController> _logger;

        public UserRoleManagementController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IAdminDashboardService adminDashboardService,
            ILogger<UserRoleManagementController> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _adminDashboardService = adminDashboardService;
            _logger = logger;
        }

        // GET: Admin/UserRoleManagement
        public async Task<IActionResult> Index()
        {
            var users = _userManager.Users.ToList();
            var roles = await _roleManager.Roles.ToListAsync();
            var userRoleViewModels = new List<UserRoleViewModel>();

            foreach (var user in users)
            {
                var userRoles = await _userManager.GetRolesAsync(user);
                var userRoleViewModel = new UserRoleViewModel
                {
                    UserId = user.Id,
                    UserName = user.UserName,
                    Email = user.Email,
                    Roles = (List<string>)userRoles
                };
                userRoleViewModels.Add(userRoleViewModel);
            }

            ViewBag.Roles = roles;
            return View(userRoleViewModels);
        }

        // GET: Admin/UserRoleManagement/AssignRole/5
        // GET: Admin/UserRoleManagement/AssignRole/5
        [HttpGet]
        public async Task<IActionResult> AssignRole(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning($"User with ID {userId} not found.");
                return NotFound();
            }

            var roles = await _roleManager.Roles.ToListAsync();
            var userRoles = await _userManager.GetRolesAsync(user);

            var model = new AssignRoleViewModel
            {
                UserId = user.Id,
                UserName = user.UserName,
                ExistingRoles = userRoles.ToList(), // Ensure ExistingRoles is populated
                AvailableRoles = roles.Where(r => !userRoles.Contains(r.Name)).ToList(),
                RolesToAdd = new List<string>() // Initialize RolesToAdd to avoid null errors
            };

            return View(model);
        }

        // POST: Admin/UserRoleManagement/AssignRole
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignRole(AssignRoleViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByIdAsync(model.UserId);
                if (user == null)
                {
                    return NotFound();
                }

                // Get the current roles of the user
                var existingRoles = await _userManager.GetRolesAsync(user);

                // Remove the roles that were unselected
                var rolesToRemove = existingRoles.Where(r => !model.RolesToAdd.Contains(r)).ToList();
                foreach (var role in rolesToRemove)
                {
                    await _userManager.RemoveFromRoleAsync(user, role);
                }

                // Add the new roles
                foreach (var role in model.RolesToAdd)
                {
                    await _userManager.AddToRoleAsync(user, role);
                }

                return RedirectToAction("Index");  // Redirect to the user role management page after successful update
            }

            // In case of errors, return the model to the view to display validation errors
            return View(model);
        }


        // GET: Admin/UserRoleManagement/RemoveRole/5
        [HttpGet]
        public async Task<IActionResult> RemoveRole(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            var roles = await _userManager.GetRolesAsync(user);
            var model = new RemoveRoleViewModel
            {
                UserId = userId,
                Roles = roles.ToList(),
                RolesToRemove = new List<string>() // Ensure this is initialized
            };

            return View(model);
        }


        // POST: Admin/UserRoleManagement/RemoveRole
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> RemoveRole(RemoveRoleViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);  // Return the model to the view with validation errors
            }

            // Ensure RolesToRemove is populated
            if (model.RolesToRemove != null && model.RolesToRemove.Any())
            {
                var user = await _userManager.FindByIdAsync(model.UserId);
                if (user == null)
                {
                    return NotFound();
                }

                foreach (var role in model.RolesToRemove)
                {
                    var result = await _userManager.RemoveFromRoleAsync(user, role);
                    if (!result.Succeeded)
                    {
                        // Handle error, maybe add error messages to ModelState
                    }
                }
            }

            // After removing roles, you can redirect or show success message
            return RedirectToAction("Index");  // Redirect after successful removal
        }

        // GET: Admin/UserRoleManagement/CreateRole
        public IActionResult CreateRole()
        {
            return View();
        }

        // POST: Admin/UserRoleManagement/CreateRole
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateRole(CreateRoleViewModel model)
        {
            if (ModelState.IsValid)
            {
                var role = new IdentityRole(model.RoleName);
                var result = await _roleManager.CreateAsync(role);

                if (result.Succeeded)
                {
                    return RedirectToAction(nameof(Index));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }

            return View(model);
        }
    }
}
