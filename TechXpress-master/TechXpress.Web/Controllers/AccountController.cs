using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Encodings.Web;
using TechXpress.Data.Models;
using TechXpress.Services.OrdersService;
using TechXpress.Web.Models;
using TechXpress.Web.Services.Interfaces;
using TechXpress.Web.ViewModels;  // Assuming you have a ViewModel for login

namespace TechXpress.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IOrderService _orderService;
        private readonly IUserService _userService;
        private readonly IEmailSender _emailSender;
        private readonly ILogger<AccountController> _logger;


        public AccountController(UserManager<ApplicationUser> userManager,
            IUserService userService,
            IOrderService orderService,
            IEmailSender emailSender,
            ILogger<AccountController> logger,
            SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _orderService = orderService;
            _userService = userService;
            _logger = logger;
        }

        // GET: /Account/Login
        public IActionResult Login() => View();

        // POST: /Account/Login
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Invalid login attempt. Please try again.";
                return View(model);  // Invalid model
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                ModelState.AddModelError("", "Invalid login attempt.");
                return View(model);
            }

            // First: authenticate user credentials
            var result = await _signInManager.PasswordSignInAsync(user.UserName, model.Password, model.RememberMe, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                // Optional: Update last login timestamp
                user.LastLogin = DateTime.UtcNow;
                await _userManager.UpdateAsync(user);

                // Create additional claims
                var claims = new List<Claim>
        {
            new Claim("ProfilePictureUrl", user.ProfilePictureUrl ?? "/images/download.jpeg")
        };

                // Sign out basic identity session and sign in again with claims
                await _signInManager.SignOutAsync();
                await _signInManager.SignInWithClaimsAsync(user, model.RememberMe, claims);
                //Welcome by name
                TempData["SuccessMessage"] = $"Welcome Back {user.FirstName} !";

                //Check User Role to Redirect
                var roles = await _userManager.GetRolesAsync(user);
                if (roles.Contains("Admin"))
                {
                    return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
                }
                else if (roles.Contains("Customer"))
                {
                    return RedirectToAction("Index", "Home");
                }



                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError("", "Invalid login attempt.");
            return View(model);
        }

        // GET: /Account/Logout
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login");
        }


        public IActionResult Register() => View();

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                // Assign "Customer" role to the new user
                await _userManager.AddToRoleAsync(user, "Customer");

                user.AccountCreated = DateTime.UtcNow;
                await _signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToAction("Index", "Home");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View(model);
        }


        [HttpGet]
        public async Task<IActionResult> Manage()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var orders = await _orderService.GetOrdersByUserAsync(user.Id);

            var model = new AccountViewModel
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                FirstName = user.FirstName,
                LastName = user.LastName,
                ProfilePictureUrl = user.ProfilePictureUrl ?? "/images/download.jpeg",
                AddressLine1 = user.AddressLine1 ?? "Not Provided",
                AddressLine2 = user.AddressLine2 ?? string.Empty,
                City = user.City ?? "Unknown City",
                State = user.State ?? "Unknown State",
                PostalCode = user.PostalCode ?? "00000",
                Country = user.Country ?? "Unknown Country",
                AccountCreated = user.AccountCreated ?? DateTime.UtcNow,
                LastLogin = user.LastLogin ?? DateTime.UtcNow, // or null / user.LastLogin.HasValue ? user.LastLogin.Value : DateTime.UtcNow
                EmailConfirmed = user.EmailConfirmed,
                TwoFactorEnabled = await _userManager.GetTwoFactorEnabledAsync(user),
                AccountStatus = user.LockoutEnd > DateTimeOffset.Now ? "Locked" : "Active",
                OrderHistory = orders.Select(o => new OrderSummary
                {
                    OrderId = o.Id,
                    OrderDate = o.OrderDate,
                    TotalAmount = o.TotalAmount,
                    Status = o.OrderStatus.ToString()
                }).ToList()
            };

            return View(model);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(AccountViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            ModelState.Remove(nameof(model.Id));
            ModelState.Remove(nameof(model.UserName));
            ModelState.Remove(nameof(model.AccountStatus));
            ModelState.Remove(nameof(model.ProfilePictureUrl));
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (ModelState.IsValid)
            {
                user.FirstName = model.FirstName;
                user.LastName = model.LastName;
                user.PhoneNumber = model.PhoneNumber;
                user.AddressLine1 = model.AddressLine1;
                user.AddressLine2 = model.AddressLine2;
                user.City = model.City;
                user.State = model.State;
                user.PostalCode = model.PostalCode;
                user.Country = model.Country;
                user.ProfilePictureUrl = model.ProfilePictureUrl;

                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    TempData["SuccessMessage"] = "Your profile has been updated";
                    return RedirectToAction(nameof(Manage));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // If we got this far, something failed, redisplay form
            return View("Manage", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateEmail(AccountViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                return View("Manage", model);
            }

            var email = await _userManager.GetEmailAsync(user);
            if (model.Email != email)
            {
                var setEmailResult = await _userManager.SetEmailAsync(user, model.Email);
                if (!setEmailResult.Succeeded)
                {
                    foreach (var error in setEmailResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return View("Manage", model);
                }
            }

            TempData["SuccessMessage"] = "Your email has been updated";
            return RedirectToAction(nameof(Manage));
        }

        [HttpPost]
        public async Task<IActionResult> UploadProfilePicture(IFormFile profileImage)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);

            if (profileImage != null && profileImage.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/profiles");
                Directory.CreateDirectory(uploadsFolder);

                // Delete old image
                if (!string.IsNullOrEmpty(user.ProfilePictureUrl))
                {
                    var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", user.ProfilePictureUrl.TrimStart('/'));
                    if (System.IO.File.Exists(oldPath))
                    {
                        System.IO.File.Delete(oldPath);
                    }
                }

                // Save new image
                var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(profileImage.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await profileImage.CopyToAsync(stream);
                }

                // Update user profile picture URL
                var relativePath = "/images/profiles/" + uniqueFileName;
                user.ProfilePictureUrl = relativePath;
                await _userManager.UpdateAsync(user);

                // 🔁 Update claims
                var currentClaims = await _userManager.GetClaimsAsync(user);
                var oldClaim = currentClaims.FirstOrDefault(c => c.Type == "ProfilePictureUrl");
                if (oldClaim != null)
                {
                    await _userManager.RemoveClaimAsync(user, oldClaim);
                }

                var newClaim = new Claim("ProfilePictureUrl", user.ProfilePictureUrl ?? "/images/download.jpeg");
                await _userManager.AddClaimAsync(user, newClaim);

                // 🔐 Re-sign in user to refresh claims
                await _signInManager.RefreshSignInAsync(user);

                return RedirectToAction("Manage");
            }

            return View("Profile");
        }


        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
                {
                    return RedirectToAction(nameof(ForgotPasswordConfirmation), new { email = model.Email });
                }

                var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                var callbackUrl = Url.Action(
       action: nameof(ResetPassword),
       controller: "Account",
       values: new { userId = user.Id, code, email = model.Email }, // Include email here
       protocol: Request.Scheme);

                await _emailSender.SendEmailAsync(
                    model.Email,
                    "Reset Password - TechXpress",
                    $"Please reset your password by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                return RedirectToAction(nameof(ForgotPasswordConfirmation), new { email = model.Email });
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult ForgotPasswordConfirmation(string email)
        {
            return View(new ForgotPasswordConfirmationViewModel { Email = email });
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPassword(string code = null, string email = null)
        {
            if (code == null)
            {
                return BadRequest("A code must be supplied for password reset.");
            }

            var model = new ResetPasswordViewModel
            {
                Code = code,
                Email = email // Pre-fill email if provided
            };
            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return RedirectToAction(nameof(ResetPasswordConfirmation));
            }

            var result = await _userManager.ResetPasswordAsync(user, model.Code, model.Password);
            if (result.Succeeded)
            {
                return RedirectToAction(nameof(ResetPasswordConfirmation), new { email = model.Email });
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPasswordConfirmation(string email)
        {
            return View(new ResetPasswordConfirmationViewModel { Email = email });
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            // Log the access denied attempt
            _logger.LogWarning($"Access denied for user {User.Identity?.Name} at {DateTime.UtcNow}");

            // You can customize the message based on the user's roles if needed
            var userRoles = User.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value);
            ViewData["UserRoles"] = string.Join(", ", userRoles);

            return View();
        }
    }
    }
