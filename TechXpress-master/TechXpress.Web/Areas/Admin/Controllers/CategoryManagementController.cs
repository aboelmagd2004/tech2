using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using TechXpress.Data.Models;
using TechXpress.Services.CategoriesService;
using TechXpress.Web.Areas.Admin.Services;

namespace TechXpress.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class CategoryManagementController : Controller
    {
        private readonly ICategoryService _categoryService;
        private readonly IImageService _imageService;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public CategoryManagementController(
            IImageService imageService,
            IWebHostEnvironment webHostEnvironment,
            ICategoryService categoryService)
        {
            _categoryService = categoryService;
            _imageService = imageService;
            _webHostEnvironment = webHostEnvironment;

        }

        // GET: Admin/CategoryManagement
        public async Task<IActionResult> Index()
        {
            var categories = await _categoryService.GetAllWithProductsAsync();
            foreach (var item in categories)
            {
                item.ProductCount = item.Products.Count;

            }
            return View(categories);
        }

        // GET: Admin/CategoryManagement/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var category = await _categoryService.GetByIdWithProductsAsync(id);
            if (category == null)
                return NotFound();

            return View(category);
        }

        // GET: Admin/CategoryManagement/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.ParentCategories = new SelectList(await _categoryService.GetAllAsync(), "Id", "Name");

            return View();
        }

        // POST: Admin/CategoryManagement/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Category model, IFormFile imageFile)
        {
            // Remove fields that shouldn't be validated
            ModelState.Remove("id");
            ModelState.Remove("productCount");
            ModelState.Remove("ImageUrl");

            // Validate image file
            if (imageFile == null || imageFile.Length == 0)
            {
                ModelState.AddModelError("imageFile", "Please select an image file");
            }
            else if (imageFile.Length > 5 * 1024 * 1024) // 5MB limit
            {
                ModelState.AddModelError("imageFile", "The image file is too large (max 5MB)");
            }
            else
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var fileExtension = Path.GetExtension(imageFile.FileName).ToLower();
                if (!allowedExtensions.Contains(fileExtension))
                {
                    ModelState.AddModelError("imageFile", "Only image files (JPG, PNG, GIF) are allowed");
                }
            }

            if (ModelState.IsValid)
            {
                model.ImageUrl = await _imageService.GetPathAsync(imageFile);
                await _categoryService.AddAsync(model);
                return RedirectToAction(nameof(Index));
            }

            // Repopulate dropdown if validation fails
            ViewBag.ParentCategories = new SelectList(await _categoryService.GetAllAsync(), "Id", "Name");
            return View(model);
        }

        // GET: Admin/CategoryManagement/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var category = await _categoryService.GetByIdAsync(id);
            if (category == null)
                return NotFound();
            ViewBag.ParentCategories = new SelectList(await _categoryService.GetAllAsync(), "Id", "Name");

            return View(category);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Category model, IFormFile imageFile)
        {
            if (id != model.Id)
                return NotFound();

            // Remove fields that shouldn't be validated
            ModelState.Remove("productCount");
            ModelState.Remove("CreatedDate");
            ModelState.Remove("imageFile");


            // Validate image file if a new one was uploaded
            if (imageFile != null && imageFile.Length > 0)
            {
                if (imageFile.Length > 5 * 1024 * 1024) // 5MB limit
                {
                    ModelState.AddModelError("imageFile", "The image file is too large (max 5MB)");
                }
                else
                {
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                    var fileExtension = Path.GetExtension(imageFile.FileName).ToLower();
                    if (!allowedExtensions.Contains(fileExtension))
                    {
                        ModelState.AddModelError("imageFile", "Only image files (JPG, PNG, GIF) are allowed");
                    }
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingCategory = await _categoryService.GetByIdAsync(id);
                    if (existingCategory == null)
                        return NotFound();

                    // Update image if new one was uploaded
                    if (imageFile != null && imageFile.Length > 0)
                    {
                        // Delete old image if exists
                        if (!string.IsNullOrEmpty(existingCategory.ImageUrl))
                        {
                            var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, existingCategory.ImageUrl.TrimStart('/'));
                            if (System.IO.File.Exists(oldImagePath))
                            {
                                System.IO.File.Delete(oldImagePath);
                            }
                        }
                        model.ImageUrl = await _imageService.GetPathAsync(imageFile);
                    }

                    // Update properties
                    existingCategory.Name = model.Name;
                    existingCategory.Slug = model.Slug;
                    existingCategory.MetaTitle = model.MetaTitle;
                    existingCategory.MetaKeywords = model.MetaKeywords;
                    existingCategory.MetaDescription = model.MetaDescription;
                    existingCategory.Description = model.Description;
                    existingCategory.ParentId = model.ParentId;
                    existingCategory.DisplayOrder = model.DisplayOrder;
                    existingCategory.IsActive = model.IsActive;
                    existingCategory.Level = model.Level;
                    existingCategory.ModifiedDate = DateTime.UtcNow;

                    // Only update image if a new one was provided
                    if (!string.IsNullOrEmpty(model.ImageUrl))
                    {
                        existingCategory.ImageUrl = model.ImageUrl;
                    }

                    await _categoryService.UpdateAsync(existingCategory);
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    if (!await CategoryExists(model.Id))
                    {
                        return NotFound();
                    }
                    ModelState.AddModelError(string.Empty, "The record you attempted to edit was modified by another user. Please refresh and try again.");
                }
                catch (Exception ex)
                {
                    // Log the error
                    //_logger.LogError(ex, "Error editing category");
                    ModelState.AddModelError(string.Empty, "An error occurred while saving. Please try again.");
                }
            }

            // If we got this far, something failed; redisplay form
            ViewBag.ParentCategories = new SelectList(await _categoryService.GetAllAsync(), "Id", "Name", model.ParentId);
            return View(model);
        }

        private async Task<bool> CategoryExists(int id)
        {
            return (await _categoryService.GetByIdAsync(id)) != null;
        }

        // GET: Admin/CategoryManagement/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _categoryService.GetByIdAsync(id);
            if (category == null)
                return NotFound();

            return View(category);
        }

        // POST: Admin/CategoryManagement/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _categoryService.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }

    }
}
