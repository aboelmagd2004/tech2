using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using TechXpress.Data.Models;
using TechXpress.Services.Payment; // Import the Stripe service
using TechXpress.Services.GenericServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Stripe;

namespace TechXpress.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class PaymentManagementController : Controller
    {
        private readonly IGenericService<Payment> _paymentService;
        private readonly IStripeService _stripeService;
        private readonly ILogger<PaymentManagementController> _logger;

        // Constructor injection of the services
        public PaymentManagementController(
            IGenericService<Payment> paymentService,
            IStripeService stripeService,
            ILogger<PaymentManagementController> logger)
        {
            _paymentService = paymentService;
            _stripeService = stripeService;
            _logger = logger;
        }

        // GET: Admin/PaymentManagement/Index
        public async Task<IActionResult> Index()
        {
            var payments = await _paymentService.GetAllAsync(); // Get all payments from the database
            return View(payments); // Display a list of payments
        }

        // GET: Admin/PaymentManagement/Details/{paymentId}
        public async Task<IActionResult> Details(int paymentId)
        {
            var payment = await _paymentService.GetByIdAsync(paymentId);
            if (payment == null)
            {
                return NotFound();
            }
            return View(payment); // Display payment details
        }

        // GET: Admin/PaymentManagement/Process/{paymentId}
        public async Task<IActionResult> ProcessPayment(int paymentId)
        {
            var payment = await _paymentService.GetByIdAsync(paymentId);
            if (payment == null)
            {
                return NotFound();
            }

            // Logic to process payment, e.g., interacting with Stripe API or updating status
            try
            {
                if (payment.Status == PaymentStatus.Pending)
                {
                    // Create or retrieve the payment intent from Stripe
                    var paymentIntent = await _stripeService.CreatePaymentIntentAsync(
                        (long)(payment.Amount * 100), // Amount in smallest currency unit (e.g., cents)
                        "usd", // Currency (you can modify as needed)
                        payment.OrderId
                    );

                    // Update the payment status and transaction details
                    payment.Status = PaymentStatus.Authorized;
                    payment.TransactionId = paymentIntent.Id;
                    payment.PaymentDate = System.DateTime.UtcNow;
                    await _paymentService.UpdateAsync(payment);

                    _logger.LogInformation($"Payment {payment.Id} processed successfully via Stripe.");
                }
                else
                {
                    _logger.LogWarning($"Payment {payment.Id} is not in a Pending state.");
                    return BadRequest("Payment is not in Pending status.");
                }
            }
            catch (StripeException ex)
            {
                _logger.LogError($"Error processing payment {payment.Id}: {ex.Message}");
                return StatusCode(500, "Payment processing failed.");
            }

            return RedirectToAction(nameof(Index)); // Redirect to the payment list
        }

        // POST: Admin/PaymentManagement/Refund/{paymentId}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RefundPayment(int paymentId)
        {
            var payment = await _paymentService.GetByIdAsync(paymentId);
            if (payment == null)
            {
                return NotFound();
            }

            // Logic to refund payment via Stripe
            try
            {
                if (payment.Status == PaymentStatus.Completed)
                {
                    // Refund logic using Stripe API
                    var refundOptions = new Stripe.RefundCreateOptions
                    {
                        PaymentIntent = payment.TransactionId
                    };
                    var refundService = new Stripe.RefundService();
                    var refund = await refundService.CreateAsync(refundOptions);

                    // Update payment status to "Refunded"
                    payment.Status = PaymentStatus.Refunded;
                    payment.ProcessorResponse = refund.Status;
                    payment.ProcessedDate = System.DateTime.UtcNow;
                    await _paymentService.UpdateAsync(payment);

                    _logger.LogInformation($"Payment {payment.Id} refunded successfully via Stripe.");
                }
                else
                {
                    _logger.LogWarning($"Payment {payment.Id} is not in Completed status.");
                    return BadRequest("Only completed payments can be refunded.");
                }
            }
            catch (StripeException ex)
            {
                _logger.LogError($"Error refunding payment {payment.Id}: {ex.Message}");
                return StatusCode(500, "Refund failed.");
            }

            return RedirectToAction(nameof(Index)); // Redirect to the payment list
        }

        // POST: Admin/PaymentManagement/MarkAsFailed/{paymentId}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsFailed(int paymentId)
        {
            var payment = await _paymentService.GetByIdAsync(paymentId);
            if (payment == null)
            {
                return NotFound();
            }

            // Logic to mark payment as failed
            payment.Status = PaymentStatus.Failed;
            await _paymentService.UpdateAsync(payment);

            _logger.LogInformation($"Payment {payment.Id} marked as Failed.");
            return RedirectToAction(nameof(Index)); // Redirect to the payment list
        }
    }
}
