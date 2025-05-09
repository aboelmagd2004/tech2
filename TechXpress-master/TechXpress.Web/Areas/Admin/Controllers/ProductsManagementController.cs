using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Threading.Tasks;
using TechXpress.Services.ProductsService;
using TechXpress.Services.CategoriesService;
using TechXpress.Data.Models;
using TechXpress.Web.Areas.Admin.Services;
using Microsoft.AspNetCore.Http;

namespace TechXpress.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ProductsManagementController : Controller
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly IImageService _imageService;

        public ProductsManagementController(
            IProductService productService,
            IImageService imageService,
            ICategoryService categoryService)
        {
            _productService = productService;
            _categoryService = categoryService;
            _imageService = imageService;
        }

        public async Task<IActionResult> Index()
        {
            var products = await _productService.GetAllProductsWithCategoriesAsync();
            return View(products);
        }

        public async Task<IActionResult> Details(int id)
        {
            var product = await _productService.GetProductsWithCategoryAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        public async Task<IActionResult> Create()
        {
            await PopulateCategoriesAsync();
            return View(new ProductViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductViewModel model, IFormFile imageFile)
        {
            if (ModelState.IsValid)
            {
                if (imageFile != null && imageFile.Length > 0)
                {
                    model.ImageUrl = await _imageService.GetPathAsync(imageFile);
                }

                // Ensure Specifications is not null before calling UpdateSpecifications  
                model.Specifications ??= new List<ProductSpecification>();

                _productService.UpdateSpecifications(model, model.Specifications);
                await _productService.AddAsync(model);
                return RedirectToAction(nameof(Index));
            }

            await PopulateCategoriesAsync(model.CategoryId);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ProductViewModel model, IFormFile? imageFile)
        {
            if (id != model.Id)
            {
                return BadRequest();
            }

            // Manually validate Specifications
            if (model.Specifications != null)
            {
                for (int i = 0; i < model.Specifications.Count; i++)
                {
                    var spec = model.Specifications[i];
                    if (string.IsNullOrWhiteSpace(spec.Key))
                    {
                        ModelState.AddModelError($"Specifications[{i}].Key", "The Key field is required.");
                    }
                    if (string.IsNullOrWhiteSpace(spec.Value))
                    {
                        ModelState.AddModelError($"Specifications[{i}].Value", "The Value field is required.");
                    }
                }
            }

            // Remove all Specifications entries from ModelState except the ones we just added
            var specsKeys = ModelState.Keys
                .Where(k => k.StartsWith("Specifications["))
                .ToList();

            foreach (var key in specsKeys)
            {
                // Only remove if it's not one of our manually added errors
                if (!key.EndsWith(".Key") && !key.EndsWith(".Value"))
                {
                    ModelState.Remove(key);
                }
            }
            ModelState.Remove("ImageUrl");

            if (ModelState.IsValid)
            {
                if (imageFile != null && imageFile.Length > 0)
                {
                    model.ImageUrl = await _imageService.GetPathAsync(imageFile);
                }

                // Process the specifications
                model.Specifications = model.Specifications?
                    .Where(s => !string.IsNullOrWhiteSpace(s.Key) && !string.IsNullOrWhiteSpace(s.Value))
                    .ToList() ?? new List<ProductSpecification>();

                try
                {
                    await _productService.UpdateAsync(model);
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "An error occurred while saving: " + ex.Message);
                }
            }

            await PopulateCategoriesAsync(model.CategoryId);
            return View(model);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var product = await _productService.GetProductsWithCategoryAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            await PopulateCategoriesAsync(product.CategoryId);
            return View(product);
        }

        // Add these methods to your ProductsManagementController

        [HttpGet]
        public IActionResult GetSpecificationRow()
        {
            return PartialView("_SpecificationRowPartial", new ProductSpecification());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddSpecification(int productId, ProductSpecification specification)
        {
            if (ModelState.IsValid)
            {
                specification.ProductId = productId;
                // Add logic to save the specification to your database
                // await _specificationService.AddAsync(specification);
                return RedirectToAction("Edit", new { id = productId });
            }
            await PopulateCategoriesAsync();
            return View("Edit", await _productService.GetProductsWithCategoryAsync(productId));
        }

        public async Task<IActionResult> Delete(int id)
        {
            var product = await _productService.GetProductsWithCategoryAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _productService.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }

        private async Task PopulateCategoriesAsync(int? selectedCategoryId = null)
        {
            var categories = await _categoryService.GetAllAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name", selectedCategoryId);
        }
    }
}