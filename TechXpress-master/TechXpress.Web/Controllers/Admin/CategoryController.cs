//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Mvc.Rendering;
//using System.Text;
//using TechXpress.Services.CategoriesService;
//using TechXpress.Services.ProductsService;
//using TechXpress.Web.Models;

//namespace TechXpress.Web.Controllers.Admin
//{
//    // TechXpress.Web/Areas/Admin/Controllers/CategoryController.cs
//    //[Area("Admin")]
//    //[Authorize(Roles = "Admin,SuperAdmin")]
//    public class CategoryController : Controller
//    {
//        private readonly ICategoryService _categoryService;
//        private readonly IProductService _productService;
//        private readonly IImageService _imageService;
//        private readonly ILogger<CategoryController> _logger;

//        public CategoryController(
//            ICategoryService categoryService,
//            IProductService productService,
//            IImageService imageService,
//            ILogger<CategoryController> logger)
//        {
//            _categoryService = categoryService;
//            _productService = productService;
//            _imageService = imageService;
//            _logger = logger;
//        }

//        [HttpGet]
//        public async Task<IActionResult> Index()
//        {
//            try
//            {
//                var stats = await _categoryService.GetCategoryStatisticsAsync();
//                ViewBag.CategoryStats = stats;
//                return View();
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error loading category index");
//                return View("Error");
//            }
//        }

//        [HttpGet]
//        [Route("Admin/Category/GetCategories")]
//        public async Task<IActionResult> GetCategories(DataTablesRequest request)
//        {
//            try
//            {
//                var result = await _categoryService.GetCategoriesForDataTablesAsync(
//                    request.Start,
//                    request.Length,
//                    request.Search.Value,
//                    request.Order[0].Column,
//                    request.Order[0].Dir);

//                return Json(new DataTablesResponse(
//                    request.Draw,
//                    result.Data,
//                    result.TotalRecords,
//                    result.TotalRecordsFiltered));
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error retrieving categories for DataTables");
//                return Json(new { error = "An error occurred while processing your request." });
//            }
//        }

//        [HttpGet]
//        public async Task<IActionResult> Create()
//        {
//            var model = new CategoryCreateViewModel
//            {
//                AvailableParentCategories = new SelectList(
//                    await _categoryService.GetAllCategoriesForSelectAsync(),
//                    "Id",
//                    "Name")
//            };
//            return View(model);
//        }

//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> Create(CategoryCreateViewModel model)
//        {
//            try
//            {
//                if (!ModelState.IsValid)
//                {
//                    model.AvailableParentCategories = new SelectList(
//                        await _categoryService.GetAllCategoriesForSelectAsync(),
//                        "Id",
//                        "Name",
//                        model.ParentId);
//                    return View(model);
//                }

//                // Handle image upload
//                string imageUrl = null;
//                if (model.ImageFile != null && model.ImageFile.Length > 0)
//                {
//                    imageUrl = await _imageService.UploadCategoryImageAsync(model.ImageFile);
//                }

//                var category = new CategoryDto
//                {
//                    Name = model.Name,
//                    Description = model.Description,
//                    ParentId = model.ParentId,
//                    DisplayOrder = model.DisplayOrder,
//                    ImageUrl = imageUrl,
//                    MetaTitle = model.MetaTitle,
//                    MetaDescription = model.MetaDescription,
//                    MetaKeywords = model.MetaKeywords,
//                    IsActive = true
//                };

//                await _categoryService.CreateCategoryAsync(category);

//                _logger.LogInformation("Category {CategoryName} created by {User}", model.Name, User.Identity.Name);
//                TempData["SuccessMessage"] = "Category created successfully!";
//                return RedirectToAction(nameof(Index));
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error creating category");
//                ModelState.AddModelError("", "An error occurred while creating the category.");
//                model.AvailableParentCategories = new SelectList(
//                    await _categoryService.GetAllCategoriesForSelectAsync(),
//                    "Id",
//                    "Name",
//                    model.ParentId);
//                return View(model);
//            }
//        }

//        [HttpGet]
//        [ValidateCategoryExists]
//        public async Task<IActionResult> Edit(int id)
//        {
//            var category = (CategoryDto)HttpContext.Items["category"];

//            var model = new CategoryEditViewModel
//            {
//                Id = category.Id,
//                Name = category.Name,
//                Description = category.Description,
//                ParentId = category.ParentId,
//                DisplayOrder = category.DisplayOrder,
//                ExistingImageUrl = category.ImageUrl,
//                MetaTitle = category.MetaTitle,
//                MetaDescription = category.MetaDescription,
//                MetaKeywords = category.MetaKeywords,
//                CreatedDate = category.CreatedDate,
//                ModifiedDate = category.ModifiedDate,
//                AvailableParentCategories = new SelectList(
//                    await _categoryService.GetAllCategoriesForSelectAsync(),
//                    "Id",
//                    "Name",
//                    category.ParentId)
//            };

//            return View(model);
//        }

//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        [ValidateCategoryExists]
//        public async Task<IActionResult> Edit(int id, CategoryEditViewModel model)
//        {
//            try
//            {
//                if (id != model.Id)
//                {
//                    return NotFound();
//                }

//                if (!ModelState.IsValid)
//                {
//                    model.AvailableParentCategories = new SelectList(
//                        await _categoryService.GetAllCategoriesForSelectAsync(),
//                        "Id",
//                        "Name",
//                        model.ParentId);
//                    return View(model);
//                }

//                var category = (CategoryDto)HttpContext.Items["category"];
//                string imageUrl = category.ImageUrl;

//                // Handle image upload or deletion
//                if (model.ImageFile != null && model.ImageFile.Length > 0)
//                {
//                    // Delete old image if exists
//                    if (!string.IsNullOrEmpty(imageUrl))
//                    {
//                        await _imageService.DeleteImageAsync(imageUrl);
//                    }
//                    // Upload new image
//                    imageUrl = await _imageService.UploadCategoryImageAsync(model.ImageFile);
//                }

//                // Update category properties
//                category.Name = model.Name;
//                category.Description = model.Description;
//                category.ParentId = model.ParentId;
//                category.DisplayOrder = model.DisplayOrder;
//                category.ImageUrl = imageUrl;
//                category.MetaTitle = model.MetaTitle;
//                category.MetaDescription = model.MetaDescription;
//                category.MetaKeywords = model.MetaKeywords;
//                category.ModifiedDate = DateTime.UtcNow;

//                await _categoryService.UpdateCategoryAsync(category);

//                _logger.LogInformation("Category {CategoryId} updated by {User}", id, User.Identity.Name);
//                TempData["SuccessMessage"] = "Category updated successfully!";
//                return RedirectToAction(nameof(Index));
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error updating category {CategoryId}", id);
//                ModelState.AddModelError("", "An error occurred while updating the category.");
//                model.AvailableParentCategories = new SelectList(
//                    await _categoryService.GetAllCategoriesForSelectAsync(),
//                    "Id",
//                    "Name",
//                    model.ParentId);
//                return View(model);
//            }
//        }

//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        [ValidateCategoryExists]
//        public async Task<IActionResult> Delete(int id)
//        {
//            try
//            {
//                var category = (CategoryDto)HttpContext.Items["category"];

//                // Check if category has products
//                var productCount = await _productService.GetProductCountByCategoryAsync(id);
//                if (productCount > 0)
//                {
//                    TempData["ErrorMessage"] = "Cannot delete a category that contains products. Please reassign or delete the products first.";
//                    return RedirectToAction(nameof(Index));
//                }

//                // Delete image if exists
//                if (!string.IsNullOrEmpty(category.ImageUrl))
//                {
//                    await _imageService.DeleteImageAsync(category.ImageUrl);
//                }

//                await _categoryService.DeleteCategoryAsync(id);

//                _logger.LogInformation("Category {CategoryId} deleted by {User}", id, User.Identity.Name);
//                TempData["SuccessMessage"] = "Category deleted successfully!";
//                return RedirectToAction(nameof(Index));
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error deleting category {CategoryId}", id);
//                TempData["ErrorMessage"] = "An error occurred while deleting the category.";
//                return RedirectToAction(nameof(Index));
//            }
//        }

//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> Reorder([FromBody] List<CategoryOrderUpdateDto> orderUpdates)
//        {
//            try
//            {
//                if (orderUpdates == null || !orderUpdates.Any())
//                {
//                    return Json(new { success = false, message = "No order updates provided." });
//                }

//                await _categoryService.UpdateCategoryOrderAsync(orderUpdates);

//                _logger.LogInformation("Category order updated by {User}", User.Identity.Name);
//                return Json(new { success = true, message = "Category order updated successfully." });
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error updating category order");
//                return Json(new { success = false, message = "An error occurred while updating category order." });
//            }
//        }

//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> ToggleActive(int id)
//        {
//            try
//            {
//                var result = await _categoryService.ToggleCategoryActiveStatusAsync(id);
//                if (!result)
//                {
//                    return Json(new { success = false, message = "Category not found." });
//                }

//                _logger.LogInformation("Category {CategoryId} active status toggled by {User}", id, User.Identity.Name);
//                return Json(new { success = true, message = "Category status updated successfully." });
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error toggling category active status for {CategoryId}", id);
//                return Json(new { success = false, message = "An error occurred while updating category status." });
//            }
//        }

//        [HttpPost]
//        [Route("Admin/Category/UploadImage")]
//        public async Task<IActionResult> UploadImage(IFormFile file)
//        {
//            try
//            {
//                if (file == null || file.Length == 0)
//                    return Json(new { success = false, message = "No file uploaded." });

//                var imageUrl = await _imageService.UploadCategoryImageAsync(file);
//                return Json(new { success = true, url = imageUrl });
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error uploading category image");
//                return Json(new { success = false, message = "Error uploading image." });
//            }
//        }

//        [HttpGet]
//        [ValidateCategoryExists]
//        public async Task<IActionResult> ExportProducts(int id, string format = "csv")
//        {
//            try
//            {
//                var category = (CategoryDto)HttpContext.Items["category"];
//                var products = await _productService.GetProductsByCategoryAsync(category.Id);

//                if (format.Equals("excel", StringComparison.OrdinalIgnoreCase))
//                {
//                    // Generate Excel file
//                    var excelBytes = await GenerateExcelExport(products, category.Name);
//                    return File(excelBytes,
//                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
//                        $"{category.Name.SanitizeFileName()}_products.xlsx");
//                }
//                else
//                {
//                    // Generate CSV file
//                    var csvContent = GenerateCsvExport(products);
//                    return File(Encoding.UTF8.GetBytes(csvContent),
//                        "text/csv",
//                        $"{category.Name.SanitizeFileName()}_products.csv");
//                }
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error exporting products for category {CategoryId}", id);
//                TempData["ErrorMessage"] = "An error occurred while exporting products.";
//                return RedirectToAction(nameof(Index));
//            }
//        }

//        private async Task<byte[]> GenerateExcelExport(IEnumerable<ProductViewModel> products, string categoryName)
//        {
//            using var package = new ExcelPackage();
//            var worksheet = package.Workbook.Worksheets.Add("Products");

//            // Add header row
//            worksheet.Cells[1, 1].Value = "ID";
//            worksheet.Cells[1, 2].Value = "Name";
//            worksheet.Cells[1, 3].Value = "SKU";
//            worksheet.Cells[1, 4].Value = "Price";
//            worksheet.Cells[1, 5].Value = "Stock Quantity";
//            worksheet.Cells[1, 6].Value = "Active";

//            // Add data rows
//            var row = 2;
//            foreach (var product in products)
//            {
//                worksheet.Cells[row, 1].Value = product.Id;
//                worksheet.Cells[row, 2].Value = product.Name;
//                worksheet.Cells[row, 3].Value = product.SKU;
//                worksheet.Cells[row, 4].Value = product.Price;
//                worksheet.Cells[row, 5].Value = product.StockQuantity;
//                worksheet.Cells[row, 6].Value = product.IsActive ? "Yes" : "No";
//                row++;
//            }

//            // Auto-fit columns
//            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

//            // Add title
//            worksheet.HeaderFooter.FirstHeader.CenteredText = $"Products in Category: {categoryName}";
//            worksheet.HeaderFooter.FirstFooter.CenteredText = $"Generated on {DateTime.UtcNow:yyyy-MM-dd}";

//            return await package.GetAsByteArrayAsync();
//        }

//        private string GenerateCsvExport(IEnumerable<ProductViewModel> products)
//        {
//            var sb = new StringBuilder();
//            sb.AppendLine("ID,Name,SKU,Price,Stock Quantity,Active");

//            foreach (var product in products)
//            {
//                sb.AppendLine($"\"{product.Id}\",\"{product.Name.EscapeCsvField()}\",\"{product.SKU.EscapeCsvField()}\",{product.Price},{product.StockQuantity},\"{(product.IsActive ? "Yes" : "No")}\"");
//            }

//            return sb.ToString();
//        }
//    }
//}
