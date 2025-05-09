using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using TechXpress.Services.ProductsService;
using TechXpress.Web.Models;

namespace TechXpress.Web.Controllers
{

    public class HomeController : Controller
    {
        private readonly IProductService _productService;

        public HomeController(IProductService productService)
        {
            _productService = productService;
        }

        public async Task<IActionResult> Index()
        {
            // Get featured or newest products to display on homepage
            var featuredProducts = await _productService.GetFeaturedProductsAsync(5);
            return View(featuredProducts);
        }

        public IActionResult About()
        {
            return View();
        }

        public IActionResult Contact()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route("Error")]
        public IActionResult Error()
        {
            var model = new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            };
            return View(model);
        }

    }

}
