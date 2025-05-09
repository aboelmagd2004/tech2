using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using TechXpress.Services.OrdersService;
using TechXpress.Services.OrdersDetailsService;
using TechXpress.Data.Models;

namespace TechXpress.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class OrderManagementController : Controller
    {
        private readonly IOrderService _orderService;
        private readonly IOrderDetailsService _orderDetailsService;

        public OrderManagementController(IOrderService orderService, IOrderDetailsService orderDetailsService)
        {
            _orderService = orderService;
            _orderDetailsService = orderDetailsService;
        }

        // GET: Admin/OrderManagement
        public async Task<IActionResult> Index()
        {
            var orders = await _orderService.GetAllAsync();
            return View(orders);
        }

        // GET: Admin/OrderManagement/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var order = await _orderService.GetOrderWithDetailsAsync(id);
            if (order == null)
                return NotFound();

            return View(order);
        }

        // POST: Admin/OrderManagement/UpdateStatus/5
        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int id, OrderStatus newStatus, string adminNotes)
        {
            await _orderService.UpdateOrderStatusAsync(id, newStatus, adminNotes);
            return RedirectToAction(nameof(Index));
        }

        // GET: Admin/OrderManagement/Stats
        public async Task<IActionResult> Stats()
        {
            var totalSales = await _orderService.GetSalesTotalAsync();
            var orderCounts = await _orderService.GetOrderStatusDistributionAsync();

            ViewBag.TotalSales = totalSales;
            ViewBag.OrderCounts = orderCounts;

            return View();
        }
    }
}
