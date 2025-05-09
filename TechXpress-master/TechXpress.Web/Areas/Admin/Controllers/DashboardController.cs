using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechXpress.Web.Areas.Admin.Models;
using TechXpress.Web.Areas.Admin.Services;

namespace TechXpress.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class DashboardController : Controller
    {
        private readonly IAdminDashboardService _dashboardService;

        public DashboardController(IAdminDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        public IActionResult Index()
        {
            var model = _dashboardService.GetDashboardData();
            return View(model);
        }

        public IActionResult GetSalesData([FromQuery] string period = "monthly")
        {
            var data = _dashboardService.GetSalesData(period);
            return Json(data);
        }

        //public IActionResult GetUserActivity()
        //{
        //    var data = _dashboardService.GetUserActivity();
        //    return Json(data);
        //}
    }
}