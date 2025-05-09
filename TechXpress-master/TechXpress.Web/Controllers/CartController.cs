using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TechXpress.Data.Models;
using TechXpress.Services.CartItemsService;
using TechXpress.Services.ShoppingCartsService;
using TechXpress.Web.ViewModels;
using Microsoft.Extensions.Logging;
using TechXpress.Web.Extensions;
using TechXpress.Services.OrdersDetailsService;
using TechXpress.Services.OrdersService;
using Microsoft.EntityFrameworkCore;
using TechXpress.Services.ProductsService;
using TechXpress.Data.UnitOfWork;
using TechXpress.Services.Payment;
//using TechXpress.Web.Extensions;

namespace TechXpress.Web.Controllers
{
    [Route("cart")]
    public class CartController : Controller
    {
        private readonly IShoppingCartService _cartService;
        private readonly ICartItemService _cartItemService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<CartController> _logger;
        private readonly IOrderDetailsService _orderDetailsService;
        private readonly IOrderService _orderService;
        private readonly IProductService _productService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IStripeService _stripeService;

        private const string SessionCartIdKey = "CartId";

        public CartController(
            IShoppingCartService cartService,
            ICartItemService cartItemService,
            IHttpContextAccessor httpContextAccessor,
            IOrderDetailsService orderDetailsService,
            IOrderService orderService,
            IProductService  productService,
            IUnitOfWork unitOfWork,
            IStripeService stripeService,


            ILogger<CartController> logger)
        {
            _cartService = cartService;
            _cartItemService = cartItemService;
            _httpContextAccessor = httpContextAccessor;
            _orderDetailsService = orderDetailsService;
            _orderService = orderService;
            _productService = productService;
            _stripeService = stripeService;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        // ---------------------------- Core Actions ----------------------------

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            try
            {
                var cart = await GetOrCreateCartAsync(includeItems: true);
                if (cart == null || cart.Items == null || !cart.Items.Any())
                    return View("Empty");

                var cartViewModel = new CartViewModel
                {
                    CartId = cart.Id.ToString(),
                    Items = cart.Items.Select(item => new CartItemViewModel
                    {
                        ItemId = item.Id.ToString(),
                        Quantity = item.Quantity,
                        Product = new ProductViewModel
                        {
                            Id = item.ProductId,
                            Name = item.Product?.Name,
                            ImageUrl = item.Product?.ImageUrl,
                            Price = item.Product?.Price ?? 0m,
                            StockQuantity = item.Product?.StockQuantity ?? 0
                        },
                        TotalPrice = (int)((item.Product?.Price ?? 0m) * item.Quantity)
                    }).ToList(),
                    Subtotal = cart.Items.Sum(item => (item.Product?.Price ?? 0m) * item.Quantity),
                    Tax = 0m, // Will be calculated
                    Shipping = 0m, // Will be calculated
                    Total = 0m // Will be calculated
                };

                // Calculate order totals
                cartViewModel.Tax = cartViewModel.Subtotal * 0.1m; // Example 10% tax
                cartViewModel.Shipping = cartViewModel.Subtotal > 50 ? 0m : 5.99m; // Free shipping over $50
                cartViewModel.Total = cartViewModel.Subtotal + cartViewModel.Tax + cartViewModel.Shipping;

                //ViewBag.CartItemCount
                ViewBag.CartItemCount = cart.Items.Sum(item => item.Quantity);



                return View(cartViewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading cart");
                TempData["ErrorMessage"] = "There was an error loading your cart. Please try again.";
                return View("Empty");
            }
        }

        [HttpPost("add")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToCart([FromForm] AddToCartRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                // Retrieve the product from the database
                var product = await _productService.GetByIdAsync(request.ProductId);

                // Check if the product exists
                if (product == null)
                {
                    return NotFound("Product not found");
                }

                // Check if the requested quantity is available
                if (request.Quantity > product.StockQuantity)
                {
                    // Return a message indicating insufficient stock
                    if (Request.IsAjaxRequest())
                    {
                        return Json(new { success = false, message = $"Only {product.StockQuantity} items are in stock" });
                    }

                    TempData["ErrorMessage"] = $"Only {product.StockQuantity} items are available in stock.";
                    return RedirectToAction("Index");
                }

                var cart = await GetOrCreateCartAsync();
                await _cartService.AddItemToCartAsync(cart.Id, request.ProductId, request.Quantity);

                if (Request.IsAjaxRequest())
                {
                    var cartSummary = await GetCartSummary();
                    return Json(new
                    {
                        success = true,
                        message = "Item added to cart",
                        cartItemCount = cartSummary.ItemCount,
                        cartSubtotal = cartSummary.Subtotal.ToCurrencyString()
                    });
                }

                ViewBag.CartItemCount = cart.Items.Sum(item => item.Quantity);
                TempData["SuccessMessage"] = "Item added to cart successfully";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding item to cart");

                if (Request.IsAjaxRequest())
                {
                    return Json(new { success = false, message = "Error adding item to cart" });
                }

                TempData["ErrorMessage"] = "There was an error adding the item to your cart.";
                return RedirectToAction("Index");
            }
        }


        [HttpPost("update")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateQuantity([FromForm] UpdateCartItemRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var cart = await GetOrCreateCartAsync();
                await _cartService.UpdateItemQuantityAsync(cart.Id, request.ProductId, request.Quantity);

                if (Request.IsAjaxRequest())
                {
                    var cartSummary = await GetCartSummary();
                    var updatedItem = cartSummary.Items.FirstOrDefault(i => i.ProductId == request.ProductId);

                    return Json(new
                    {
                        success = true,
                        itemTotal = (updatedItem?.Price * updatedItem?.Quantity ?? 0).ToCurrencyString(),
                        cartItemCount = cartSummary.ItemCount,
                        cartSubtotal = cartSummary.Subtotal.ToCurrencyString(),
                        cartTax = cartSummary.Tax.ToCurrencyString(),
                        cartShipping = cartSummary.Shipping.ToCurrencyString(),
                        cartTotal = cartSummary.Total.ToCurrencyString()
                    });
                }
                ViewBag.CartItemCount = cart.Items.Sum(item => item.Quantity);

                TempData["SuccessMessage"] = "Cart updated successfully";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating cart item quantity");

                if (Request.IsAjaxRequest())
                {
                    return Json(new { success = false, message = "Error updating item quantity" });
                }

                TempData["ErrorMessage"] = "There was an error updating your cart.";
                return RedirectToAction("Index");
            }
        }

        [HttpPost("remove")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveItem([FromForm] RemoveCartItemRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var cart = await GetOrCreateCartAsync();
                await _cartService.RemoveItemFromCartAsync(cart.Id, request.ProductId);

                if (Request.IsAjaxRequest())
                {
                    var cartSummary = await GetCartSummary();
                    return Json(new
                    {
                        success = true,
                        message = "Item removed from cart",
                        cartItemCount = cartSummary.ItemCount,
                        cartSubtotal = cartSummary.Subtotal.ToCurrencyString(),
                        cartTax = cartSummary.Tax.ToCurrencyString(),
                        cartShipping = cartSummary.Shipping.ToCurrencyString(),
                        cartTotal = cartSummary.Total.ToCurrencyString()
                    });
                }

                ViewBag.CartItemCount = cart.Items.Sum(item => item.Quantity);


                TempData["SuccessMessage"] = "Item removed from cart successfully";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing item from cart");

                if (Request.IsAjaxRequest())
                {
                    return Json(new { success = false, message = "Error removing item from cart" });
                }

                TempData["ErrorMessage"] = "There was an error removing the item from your cart.";
                return RedirectToAction("Index");
            }
        }

        [HttpPost("clear")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ClearCart()
        {
            try
            {
                var cart = await GetOrCreateCartAsync();
                await _cartService.ClearCartAsync(cart.Id);

                if (Request.IsAjaxRequest())
                {
                    return Json(new { success = true, message = "Cart cleared successfully" });
                }

                ViewBag.CartItemCount = 0;

                TempData["SuccessMessage"] = "Cart cleared successfully";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing cart");

                if (Request.IsAjaxRequest())
                {
                    return Json(new { success = false, message = "Error clearing cart" });
                }

                TempData["ErrorMessage"] = "There was an error clearing your cart.";
                return RedirectToAction("Index");
            }
        }

        [HttpGet("summary")]
        public async Task<IActionResult> GetCartSummaryPartial()
        {
            try
            {
                var cartSummary = await GetCartSummary();
                return PartialView("_CartSummaryPartial", cartSummary);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading cart summary");
                return PartialView("_CartSummaryPartial", new CartSummaryViewModel());
            }
        }

        // ---------------------------- Checkout Process ----------------------------

        [Authorize]
        [HttpGet("checkout")]
        public async Task<IActionResult> Checkout()
        {
            try
            {
                var cart = await GetOrCreateCartAsync(includeItems: true);
                if (cart == null || cart.Items == null || !cart.Items.Any())
                {
                    TempData["ErrorMessage"] = "Your cart is empty";
                    return RedirectToAction("Index");
                }

                // Check product availability
                var unavailableItems = cart.Items.Where(item => item.Product?.StockQuantity < item.Quantity).ToList();
                if (unavailableItems.Any())
                {
                    TempData["ErrorMessage"] = $"Some items in your cart are no longer available in the requested quantities: {string.Join(", ", unavailableItems.Select(i => i.Product?.Name))}";
                    return RedirectToAction("Index");
                }

                // Get current user's profile data
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var user = await _unitOfWork.Context.Users.FindAsync(userId);

                // Create shipping address from user profile
                var shippingAddress = new AddressViewModel
                {
                    FirstName = user?.FirstName ?? "Not Provided",
                    LastName = user?.LastName ?? "Not Provided",
                    AddressLine1 = user?.AddressLine1 ?? "Not Provided",
                    AddressLine2 = user?.AddressLine2 ?? string.Empty,
                    City = user?.City ?? "Unknown City",
                    State = user?.State ?? "Unknown State",
                    ZipCode = user?.PostalCode ?? "00000",
                    Country = user?.Country ?? "US",
                    PhoneNumber = user?.PhoneNumber ?? "Not Provided"
                };

                var checkoutViewModel = new CheckoutViewModel
                {
                    Cart = new CartViewModel
                    {
                        CartId = cart.Id.ToString(),
                        Items = cart.Items.Select(item => new CartItemViewModel
                        {
                            ItemId = item.Id.ToString(),
                            Quantity = item.Quantity,
                            Product = new ProductViewModel
                            {
                                Id = item.ProductId,
                                Name = item.Product?.Name,
                                ImageUrl = item.Product?.ImageUrl,
                                Price = item.Product?.Price ?? 0m
                            },
                            TotalPrice = (int)((item.Product?.Price ?? 0m) * item.Quantity)
                        }).ToList(),
                        Subtotal = cart.Items.Sum(item => (item.Product?.Price ?? 0m) * item.Quantity),
                        Tax = cart.Items.Sum(item => (item.Product?.Price ?? 0m) * item.Quantity) * 0.1m, // 10% tax
                        Shipping = cart.Items.Sum(item => (item.Product?.Price ?? 0m) * item.Quantity) > 50 ? 0m : 5.99m, // Free shipping over $50
                        Total = 0m // Calculated below
                    },
                    ShippingAddress = shippingAddress,
                    BillingAddress = new AddressViewModel(),
                    
                    PaymentMethod = "CreditCard"
                };

                checkoutViewModel.Cart.Total = checkoutViewModel.Cart.Subtotal +
                                             checkoutViewModel.Cart.Tax +
                                             checkoutViewModel.Cart.Shipping;

                return View(checkoutViewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading checkout page");
                TempData["ErrorMessage"] = "There was an error loading the checkout page. Please try again.";
                return RedirectToAction("Index");
            }
        }
        //[Authorize]
        //[HttpPost("checkout")]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> ProcessCheckout([FromForm] CheckoutViewModel model)
        //{
        //    if (model.SameAsShipping)
        //    {
        //        ModelState.Remove("BillingAddress.FirstName");
        //        ModelState.Remove("BillingAddress.LastName");
        //        ModelState.Remove("BillingAddress.AddressLine1");
        //        ModelState.Remove("BillingAddress.AddressLine2");
        //        ModelState.Remove("BillingAddress.City");
        //        ModelState.Remove("BillingAddress.State");
        //        ModelState.Remove("BillingAddress.ZipCode");
        //        ModelState.Remove("BillingAddress.Country");
        //        ModelState.Remove("BillingAddress.PhoneNumber");

        //        model.BillingAddress = model.ShippingAddress;
        //    }

        //    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        //    var cart = await _cartService.GetOrCreateUserCartAsync(userId,includeItems: true);
        //    if (cart == null || cart.Items == null || !cart.Items.Any())
        //    {
        //        TempData["ErrorMessage"] = "Your cart is empty.";
        //        return RedirectToAction("Index", "Cart");
        //    }

        //    decimal subtotal = cart.Items.Sum(item => (item.Product?.Price ?? 0m) * item.Quantity);
        //    decimal tax = subtotal * 0.1m;
        //    decimal shipping = subtotal > 50 ? 0m : 5.99m;
        //    decimal totalAmount = subtotal + tax + shipping;

        //    if (!ModelState.IsValid)
        //    {
        //        model.Cart = new CartViewModel
        //        {
        //            CartId = cart.Id.ToString(),
        //            Items = cart.Items.Select(item => new CartItemViewModel
        //            {
        //                ItemId = item.Id.ToString(),
        //                Quantity = item.Quantity,
        //                Product = new ProductViewModel
        //                {
        //                    Id = item.ProductId,
        //                    Name = item.Product?.Name,
        //                    Price = item.Product?.Price ?? 0m
        //                },
        //                TotalPrice = (int)((item.Product?.Price ?? 0m) * item.Quantity)
        //            }).ToList(),
        //            Subtotal = subtotal,
        //            Tax = tax,
        //            Shipping = shipping,
        //            Total = totalAmount
        //        };

        //        ViewData["ErrorMessage"] = "There are errors in the form. Please review your information.";
        //        return View("Checkout", model);
        //    }




        //    // Use your shared DbContext from services if accessible
        //    var dbContext = _productService.GetDbContext(); // You must expose it in your service
        //    using var transaction = await dbContext.Database.BeginTransactionAsync();

        //    try
        //    {
        //        // 1. Process payment (mocked for demo)
        //        if (!ProcessPayment(model))
        //        {
        //            TempData["ErrorMessage"] = "Payment processing failed. Please try again.";
        //            return RedirectToAction("Checkout");
        //        }

        //        // 1. Check and decrease product quantity
        //        foreach (var item in cart.Items)
        //        {
        //            var product = await _productService.GetByIdAsync(item.ProductId);
        //            if (product == null || product.StockQuantity < item.Quantity)
        //            {
        //                TempData["ErrorMessage"] = $"Not enough stock for {product?.Name ?? "a product"}.";
        //                return RedirectToAction("Checkout");
        //            }

        //            product.StockQuantity -= item.Quantity;
        //            await _productService.UpdateAsync(product);
        //        }

        //        // 2. Create Order
        //        var order = new Order
        //        {
        //            UserId = userId,
        //            ShippingAddress = model.ShippingAddress,
        //            PaymentStatus = PaymentStatus.Pending,
        //            OrderStatus = OrderStatus.Pending,
        //            OrderDate = DateTime.UtcNow,
        //            TotalAmount = totalAmount
        //        };

        //        await _orderService.AddOrderAsync(order);

        //        // 3. Create OrderDetails
        //        var orderDetails = cart.Items.Select(item => new OrderDetail
        //        {
        //            OrderId = order.Id,
        //            ProductId = item.ProductId,
        //            Quantity = item.Quantity,
        //            Price = item.Product?.Price ?? 0m
        //        });

        //        await _orderDetailsService.AddRangeAsync(orderDetails);

        //        // 4. Clear Cart
        //        await _cartService.ClearCartAsync(cart.Id);

        //        // 5. Commit transaction
        //        await transaction.CommitAsync();

        //        TempData["SuccessMessage"] = "Order placed successfully!";
        //        return RedirectToAction("Index", "Orders");
        //    }
        //    catch (Exception ex)
        //    {
        //        await transaction.RollbackAsync();
        //        _logger.LogError(ex, "Error processing checkout.");
        //        TempData["ErrorMessage"] = "There was an error processing your order. Please try again.";
        //        return RedirectToAction("Checkout");
        //    }
        //}

        [Authorize]
        [HttpPost("checkout")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessCheckout([FromForm] CheckoutViewModel model)
        {
            if (model.SameAsShipping)
            {
                model.BillingAddress = model.ShippingAddress;
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var cart = await _cartService.GetOrCreateUserCartAsync(userId, includeItems: true);
            if (cart == null || cart.Items == null || !cart.Items.Any())
            {
                TempData["ErrorMessage"] = "Your cart is empty.";
                return RedirectToAction("Index", "Cart");
            }

            decimal subtotal = cart.Items.Sum(item => (item.Product?.Price ?? 0m) * item.Quantity);
            decimal tax = subtotal * 0.1m;
            decimal shipping = subtotal > 50 ? 0m : 5.99m;
            decimal totalAmount = subtotal + tax + shipping;

            if (!ModelState.IsValid)
            {
                // return checkout view with error
                model.Cart = new CartViewModel
                {
                    CartId = cart.Id.ToString(),
                    Items = cart.Items.Select(item => new CartItemViewModel
                    {
                        ItemId = item.Id.ToString(),
                        Quantity = item.Quantity,
                        Product = new ProductViewModel
                        {
                            Id = item.ProductId,
                            Name = item.Product?.Name,
                            Price = item.Product?.Price ?? 0m
                        },
                        TotalPrice = (int)((item.Product?.Price ?? 0m) * item.Quantity)
                    }).ToList(),
                    Subtotal = subtotal,
                    Tax = tax,
                    Shipping = shipping,
                    Total = totalAmount
                };
                return View("Checkout", model);
            }

            // Create order (but DO NOT decrease stock or set payment status)
            var order = new Order
            {
                UserId = userId,
                ShippingAddress = model.ShippingAddress,
                PaymentStatus = PaymentStatus.Pending,
                OrderStatus = OrderStatus.Pending,
                OrderDate = DateTime.UtcNow,
                TotalAmount = totalAmount,
                User=null
            };
            await _orderService.AddOrderAsync(order);

            // Create order details
            var orderDetails = cart.Items.Select(item => new OrderDetail
            {
                OrderId = order.Id,
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                Price = item.Product?.Price ?? 0m
            });
            await _orderDetailsService.AddRangeAsync(orderDetails);

            await _unitOfWork.CompleteAsync();
            return RedirectToAction("Index", "Payment", new { Id = order.Id});

        }

        // ---------------------------- Merge Guest Cart on Login ----------------------------

        [Authorize]
        [HttpGet("merge")]
        public async Task<IActionResult> MergeAfterLogin()
        {
            try
            {
                string userId = GetUserId();
                if (string.IsNullOrEmpty(userId))
                    return RedirectToAction("Index");

                if (HttpContext.Session.TryGetValue(SessionCartIdKey, out byte[] sessionCartIdBytes))
                {
                    var sessionCartId = System.Text.Encoding.UTF8.GetString(sessionCartIdBytes);
                    await _cartService.MergeGuestCartWithUserCartAsync(sessionCartId, userId);
                    HttpContext.Session.Remove(SessionCartIdKey);
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error merging guest cart");
                TempData["ErrorMessage"] = "There was an error merging your guest cart with your account.";
                return RedirectToAction("Index");
            }
        }

        // ---------------------------- Helper Methods ----------------------------

        private string GetUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        private async Task<ShoppingCart> GetOrCreateCartAsync(bool includeItems = false)
        {
            var userId = GetUserId();

            // Logged-in user
            if (!string.IsNullOrEmpty(userId))
            {
                return await _cartService.GetOrCreateUserCartAsync(userId, includeItems);
            }

            // Guest user (session-based)
            if (!HttpContext.Session.TryGetValue(SessionCartIdKey, out byte[] cartIdBytes))
            {
                var newCart = await _cartService.CreateGuestCartAsync();
                HttpContext.Session.SetString(SessionCartIdKey, newCart.Id.ToString());
                return await _cartService.GetCartByIdAsync(newCart.Id.ToString(), includeItems);
            }

            var cartId = System.Text.Encoding.UTF8.GetString(cartIdBytes);
            return await _cartService.GetCartByIdAsync(cartId, includeItems);
        }

        private async Task<CartSummaryViewModel> GetCartSummary()
        {
            var cart = await GetOrCreateCartAsync(includeItems: true);
            if (cart == null || cart.Items == null || !cart.Items.Any())
            {
                return new CartSummaryViewModel { ItemCount = 0, Subtotal = 0m };
            }

            var subtotal = cart.Items.Sum(item => (item.Product?.Price ?? 0m) * item.Quantity);
            var tax = subtotal * 0.1m; // 10% tax
            var shipping = subtotal > 50 ? 0m : 5.99m; // Free shipping over $50
            var total = subtotal + tax + shipping;

            return new CartSummaryViewModel
            {
                ItemCount = cart.Items.Sum(item => item.Quantity),
                Subtotal = subtotal,
                Tax = tax,
                Shipping = shipping,
                Total = total,
                Items = cart.Items.Select(item => new CartSummaryItemViewModel
                {
                    ProductId = item.ProductId,
                    ProductName = item.Product?.Name,
                    Quantity = item.Quantity,
                    Price = item.Product?.Price ?? 0m,
                    ImageUrl = item.Product?.ImageUrl
                }).ToList()
            };
        }
    }
}