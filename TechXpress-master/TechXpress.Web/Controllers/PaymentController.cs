using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TechXpress.Services.Payment;
using TechXpress.Web.Models;
using TechXpress.Services.OrdersService;
using TechXpress.Data.Models;
using TechXpress.Data.UnitOfWork;
using System.Security.Claims;
using TechXpress.Services.CartItemsService;
using TechXpress.Services.ProductsService;
using TechXpress.Services.OrdersDetailsService;
using TechXpress.Services.ShoppingCartsService;


namespace TechXpress.Web.Controllers
{
    public class PaymentController : Controller
    {
        private readonly IStripeService _stripeService;
        private readonly IOrderService _orderService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICartItemService _cartItemService;
        private readonly IProductService _productService;
        private readonly IOrderDetailsService _orderDetailsService;
        private readonly IShoppingCartService _cartService;



        public PaymentController(
            IStripeService stripeService,
            IOrderService orderService,
            ICartItemService cartItemService,
            IProductService productService,
            IOrderDetailsService orderDetailsService,
            IShoppingCartService cartService,
            IUnitOfWork unitOfWork)
        {
            _stripeService = stripeService;
            _orderService = orderService;
            _unitOfWork = unitOfWork;
            _productService = productService;
            _orderDetailsService = orderDetailsService;
            _cartService = cartService;
            _cartItemService = cartItemService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int Id)
        {
            Order order = await _orderService.GetByIdAsync(Id);
            if (order == null) return NotFound();


            ViewData["PublishableKey"] = HttpContext.RequestServices
                .GetRequiredService<StripeSettings>()
                .PublishableKey;

            ViewData["OrderId"] = Id;

            return View(order); // هنا نمرر الموديل
        }


        [HttpPost]
        public async Task<IActionResult> CreatePaymentIntent([FromBody] CreateIntentRequest req)
        {
            // 1. أنشئ الـ PaymentIntent في Stripe مع metadata للأوردر
            var intent = await _stripeService
                .CreatePaymentIntentAsync(req.Amount, req.Currency, req.OrderId);

            return Json(new
            {
                clientSecret = intent.ClientSecret,
                paymentIntentId = intent.Id
            });
        }

        public async Task<IActionResult> Confirm(string payment_intent, int orderId)
        {
            var intent = await _stripeService.RetrievePaymentIntentAsync(payment_intent);
            var userId = User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier);

            var payment = new Payment
            {
                OrderId = orderId,
                UserId = userId,
                Amount = intent.AmountReceived / 100m,
                Method = PaymentMethod.CreditCard,
                Status = intent.Status == "succeeded" ? PaymentStatus.Completed : PaymentStatus.Failed,
                TransactionId = intent.Id,
                ProcessorResponse = intent.Status,
                PaymentDate = DateTime.UtcNow,
                ProcessedDate = DateTime.UtcNow
            };

            await _unitOfWork.Payments.AddAsync(payment);

            if (intent.Status == "succeeded")
            {
                var orderDetails = await _orderDetailsService.GetDetailsForOrderAsync(orderId);

                foreach (var item in orderDetails)
                {
                    var product = item.Product; // Already loaded by Include()

                    if (product != null && product.StockQuantity >= item.Quantity)
                    {
                        product.StockQuantity -= item.Quantity;
                        await _productService.UpdateAsync(product);
                    }
                    else
                    {
                        //_logger.LogWarning($"Insufficient stock for product ID {item.ProductId}.");
                        TempData["ErrorMessage"] = $"Not enough stock for product {product?.Name ?? "Unknown"}.";
                        return RedirectToAction("Checkout", "Cart");
                    }
                }

                await _orderService.ProcessOrderPaymentAsync(orderId, intent.Id);

                var cart = await _cartService.GetOrCreateUserCartAsync(userId);
                if (cart != null)
                {
                    await _cartService.ClearCartAsync(cart.Id);
                }
            }

            await _unitOfWork.CompleteAsync();

            //return RedirectToAction("OrderConfirmation", "Orders", new { id = orderId });
            return RedirectToAction("Index", "Orders");

        }

    }
}