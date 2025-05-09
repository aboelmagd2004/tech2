//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;
//using TechXpress.Web.Areas.Admin.Models;
//using TechXpress.Web.Areas.Admin.Services;
//using System.Threading.Tasks;

//namespace TechXpress.Web.Areas.Admin.Controllers
//{
//    [Area("Admin")]
//    [Authorize(Roles = "Admin,SuperAdmin")]
//    public class ReportController : Controller
//    {
//        private readonly IAdminDashboardService _adminDashboardService;

//        // Constructor to inject the service layer
//        public ReportController(IAdminDashboardService adminDashboardService)
//        {
//            _adminDashboardService = adminDashboardService;
//        }

//        // GET: Admin/Report/Index
//        public async Task<IActionResult> Index()
//        {
//            // Get the data for the dashboard or reports
//            var reportData = await _adminDashboardService.GetDashboardData();
//            return View(reportData); // Pass the data to the view
//        }

//        // GET: Admin/Report/SalesReport?period=monthly
//        public async Task<IActionResult> SalesReport(string period)
//        {
//            // Get the sales data for the given period (e.g., monthly, yearly)
//            var salesData = await _adminDashboardService.GetSalesData(period);
//            return View(salesData); // Pass the sales data to the view
//        }

//        // GET: Admin/Report/OrderReport
//        public IActionResult OrderReport()
//        {
//            // Return a report view for order-related data (you can generate order-specific data)
//            return View();
//        }

//        // POST: Admin/Report/GenerateCustomReport
//        [HttpPost]
//        public async Task<IActionResult> GenerateCustomReport(CustomReportViewModel model)
//        {
//            // Assuming the service can generate custom reports based on user input
//            if (ModelState.IsValid)
//            {
//                var customReportData = await _adminDashboardService.GenerateCustomReport(model);
//                return View("CustomReport", customReportData); // Show the custom report
//            }

//            // Return the form with validation errors
//            return View(model);
//        }
//    }
//}
