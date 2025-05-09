using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechXpress.Data.Models;
using TechXpress.Services.OrdersService;
using TechXpress.Services.OrdersDetailsService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;

namespace TechXpress.Controllers
{
    [Authorize]
    public class OrdersController : Controller
    {
        private readonly IOrderService _orderService;
        private readonly IOrderDetailsService _orderDetailsService;

        public OrdersController(IOrderService orderService, IOrderDetailsService orderDetailsService)
        {
            _orderService = orderService;
            _orderDetailsService = orderDetailsService;
        }

        // Display all orders for the user
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var orders = await _orderService.GetOrdersByUserAsync(userId);
            return View(orders);
        }

        // Display order details
        public async Task<IActionResult> Details(int id)
        {
            var order = await _orderService.GetOrderWithDetailsAsync(id);

            //if (order == null || order.UserId != User.Identity.Name && !User.IsInRole("Admin"))
            //{
            //    TempData["ErrorMessage"] = "Order not found or you don't have permission to view it.";
            //    return RedirectToAction("Index");
            //}

            return View(order);
        }

        // Process payment for an order
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessPayment(int id, string paymentTransactionId)
        {
            try
            {
                var order = await _orderService.GetByIdAsync(id);

                //if (order == null || order.UserId != User.Identity.Name)
                //{
                //    TempData["ErrorMessage"] = "Order not found or you don't have permission to process payment.";
                //    return RedirectToAction("Index");
                //}

                if (order.OrderStatus != OrderStatus.Pending)
                {
                    TempData["ErrorMessage"] = "Payment can only be processed for pending orders.";
                    return RedirectToAction("Details", new { id });
                }

                var paymentSuccess = await _orderService.ProcessOrderPaymentAsync(id, paymentTransactionId);

                if (paymentSuccess)
                {
                    TempData["SuccessMessage"] = "Payment processed successfully! Your order is now being processed.";
                    return RedirectToAction("OrderConfirmation", new { id });
                }
                else
                {
                    TempData["ErrorMessage"] = "Payment processing failed. Please try again or contact support.";
                    return RedirectToAction("Details", new { id });
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An unexpected error occurred while processing payment.";
                return RedirectToAction("Details", new { id });
            }
        }

        // Update order status
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateOrderStatus(int id, OrderStatus status, string adminNotes = null)
        {
            try
            {
                var order = await _orderService.GetByIdAsync(id);

                if (order == null)
                {
                    TempData["ErrorMessage"] = "Order not found.";
                    return RedirectToAction("Index");
                }

                //Validate user permissions
                var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

                if (order.UserId != currentUserId && !User.IsInRole("Admin"))
                {
                    TempData["ErrorMessage"] = "You don't have permission to update this order.";
                    return RedirectToAction("Index");
                }

                // Additional validation for customers
                if (!User.IsInRole("Admin") && status != OrderStatus.Cancelled)
                {
                    TempData["ErrorMessage"] = "You can only cancel orders.";
                    return RedirectToAction("Details", new { id });
                }


                await _orderService.UpdateOrderStatusAsync(id, status, adminNotes);
                TempData["SuccessMessage"] = "Order status updated successfully!";

                return RedirectToAction("Details", new { id });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while updating order status.";
                return RedirectToAction("Details", new { id });
            }
        }

        // Add order note
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddOrderNote(int id, string note, bool isAdminNote = false)
        {
            try
            {
                var order = await _orderService.GetByIdAsync(id);

                if (order == null || order.UserId != User.Identity.Name && !User.IsInRole("Admin"))
                {
                    TempData["ErrorMessage"] = "Order not found or you don't have permission to add notes.";
                    return RedirectToAction("Index");
                }

                // Only admins can add admin notes
                if (isAdminNote && !User.IsInRole("Admin"))
                {
                    TempData["ErrorMessage"] = "You don't have permission to add admin notes.";
                    return RedirectToAction("Details", new { id });
                }

                await _orderService.AddOrderNoteAsync(id, note, isAdminNote);
                TempData["SuccessMessage"] = "Note added successfully!";

                return RedirectToAction("Details", new { id });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while adding note.";
                return RedirectToAction("Details", new { id });
            }
        }

        // Order confirmation page
        public async Task<IActionResult> OrderConfirmation(int id)
        {
            var order = await _orderService.GetByIdAsync(id);

            if (order == null || order.UserId != User.Identity.Name)
            {
                TempData["ErrorMessage"] = "Order not found or you don't have permission to view it.";
                return RedirectToAction("Index");
            }

            return View(order);
        }


    }
}