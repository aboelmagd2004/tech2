using Microsoft.AspNetCore.Mvc;

namespace TechXpress.Web.Controllers.Admin
{
    public class OrdersManagementController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
