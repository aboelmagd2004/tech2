using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using TechXpress.Data.Repositories.CategoryRepo;
using TechXpress.Services.CategoriesService;
using TechXpress.Services.ProductsService;
using TechXpress.Web.Models;
using TechXpress.Web.ViewModels.Products;

namespace TechXpress.Web.Controllers
{
    [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Any)]
    public class CategoryController : Controller
    {
        private readonly ICategoryService _categoryService;
        private readonly IProductService _productService;
        private readonly IMemoryCache _cache;
        private readonly ILogger<CategoryController> _logger;
        private const string CategoriesCacheKey = "CategoriesList";

        public CategoryController(
            ICategoryService categoryService,
            IProductService productService,
            IMemoryCache cache,
            ILogger<CategoryController> logger)
        {
            _categoryService = categoryService;
            _productService = productService;
            _cache = cache;
            _logger = logger;
        }

        [HttpGet]
        [ResponseCache(Duration = 120, VaryByQueryKeys = new[] { "page", "pageSize" })]
        public async Task<IActionResult> Index(int page = 1, int pageSize = 12)
        {
            try
            {
                if (!_cache.TryGetValue(CategoriesCacheKey, out List<CategoryViewModel> categories))
                {
                    var activeCategories = await _categoryService.GetAllActiveCategoriesAsync();
                    categories = activeCategories.Select(c => new CategoryViewModel
                    {
                        Id = c.Id,
                        Name = c.Name,
                        Slug = c.Slug,
                        Description = c.Description,
                        ImageUrl = c.ImageUrl,
                        ProductCount = c.ProductCount,
                        DisplayOrder = c.DisplayOrder
                    })
                    .OrderBy(c => c.DisplayOrder)
                    .ToList();

                    var cacheOptions = new MemoryCacheEntryOptions()
                        .SetSlidingExpiration(TimeSpan.FromMinutes(30))
                        .RegisterPostEvictionCallback((key, value, reason, state) =>
                        {
                            _logger.LogInformation("Cache entry {CacheKey} was evicted due to {Reason}", key, reason);
                        });

                    _cache.Set(CategoriesCacheKey, categories, cacheOptions);
                }

                var paginatedCategories = PaginatedList<CategoryViewModel>.Create(categories, page, pageSize);
                var model = new CategoryListViewModel
                {
                    Categories = paginatedCategories,
                    TotalCount = categories.Count
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving categories");
                return View("Error");
            }
        }

        [HttpGet]
        [Route("category/{slug}")]
        [ResponseCache(Duration = 60, VaryByQueryKeys = new[] { "slug", "page", "pageSize", "sortBy" })]
        public async Task<IActionResult> Details(string slug, int page = 1, int pageSize = 9, string sortBy = "newest")
        {
            try
            {
                if (string.IsNullOrEmpty(slug))
                    return BadRequest("Category slug is required");

                var category = await _categoryService.GetCategoryBySlugAsync(slug);
                if (category == null || !category.IsActive)
                    return NotFound();

                page = Math.Max(1, page);
                pageSize = Math.Max(1, Math.Min(50, pageSize));

                var sortOrder = sortBy?.ToLower() switch
                {
                    "price-low-high" => ProductSortOrder.PriceLowToHigh,
                    "price-high-low" => ProductSortOrder.PriceHighToLow,
                    "name" => ProductSortOrder.Name,
                    _ => ProductSortOrder.Newest
                };

                var productViewModels = await _categoryService.GetProductsByCategoryAsync(category.Id, sortOrder);
                var paginatedProducts = PaginatedList<ProductViewModel>.Create(productViewModels, page, pageSize);

                // Start all tasks in parallel
                var parentCategoriesTask = _categoryService.GetParentCategoriesAsync(category.Id);
                var childCategoriesTask = _categoryService.GetChildCategoriesAsync(category.Id);
                var relatedCategoriesTask = _categoryService.GetRelatedCategoriesAsync(category.Id);

                try
                {
                    await Task.WhenAll(parentCategoriesTask, childCategoriesTask, relatedCategoriesTask);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "One or more category-related tasks failed for category ID {CategoryId}", category.Id);

                    if (parentCategoriesTask.IsFaulted)
                        _logger.LogError(parentCategoriesTask.Exception, "Parent categories task failed");

                    if (childCategoriesTask.IsFaulted)
                        _logger.LogError(childCategoriesTask.Exception, "Child categories task failed");

                    if (relatedCategoriesTask.IsFaulted)
                        _logger.LogError(relatedCategoriesTask.Exception, "Related categories task failed");
                }

                var parentCategories = parentCategoriesTask.Status == TaskStatus.RanToCompletion
                    ? parentCategoriesTask.Result?.Select(c => new CategoryViewModel
                    {
                        Id = c.Id,
                        Name = c.Name ?? string.Empty,
                        Slug = c.Slug ?? string.Empty
                    }).ToList() ?? new List<CategoryViewModel>()
                    : new List<CategoryViewModel>();

                var childCategories = childCategoriesTask.Status == TaskStatus.RanToCompletion
                    ? childCategoriesTask.Result?.Select(c => new CategoryViewModel
                    {
                        Id = c.Id,
                        Name = c.Name ?? string.Empty,
                        Slug = c.Slug ?? string.Empty,
                        ProductCount = c.ProductCount
                    }).ToList() ?? new List<CategoryViewModel>()
                    : new List<CategoryViewModel>();

                var relatedCategories = relatedCategoriesTask.Status == TaskStatus.RanToCompletion
                    ? relatedCategoriesTask.Result?.Select(c => new CategoryViewModel
                    {
                        Id = c.Id,
                        Name = c.Name ?? string.Empty,
                        Slug = c.Slug ?? string.Empty,
                        ProductCount = c.ProductCount
                    }).ToList() ?? new List<CategoryViewModel>()
                    : new List<CategoryViewModel>();

                var model = new CategoryDetailsViewModel
                {
                    Id = category.Id,
                    Name = category.Name ?? string.Empty,
                    Slug = category.Slug ?? string.Empty,
                    Description = category.Description ?? string.Empty,
                    ImageUrl = category.ImageUrl ?? string.Empty,
                    MetaTitle = category.MetaTitle ?? string.Empty,
                    MetaDescription = category.MetaDescription ?? string.Empty,
                    MetaKeywords = category.MetaKeywords ?? string.Empty,
                    Products = paginatedProducts,
                    ParentCategories = parentCategories,
                    ChildCategories = childCategories,
                    RelatedCategories = relatedCategories
                };

                ViewBag.Breadcrumbs = await GetBreadcrumbsAsync(category.Id);
                ViewBag.Title = string.IsNullOrEmpty(category.MetaTitle) ? category.Name : category.MetaTitle;
                ViewBag.MetaDescription = category.MetaDescription;
                ViewBag.MetaKeywords = category.MetaKeywords;

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving category details for slug {Slug}", slug);

                var errorModel = new ErrorViewModel
                {
                    RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
                };

                return View("Error", errorModel);
            }
        }





        [HttpGet]
        [Route("api/categories/search")]
        public async Task<IActionResult> Search(string term)
        {
            if (string.IsNullOrWhiteSpace(term) || term.Length < 2)
            {
                return Json(new { success = false, message = "Search term must be at least 2 characters" });
            }

            try
            {
                var categories = await _categoryService.SearchActiveCategoriesAsync(term);
                var results = categories.Select(c => new
                {
                    id = c.Id,
                    text = c.Name,
                    slug = c.Slug
                });

                return Json(new { success = true, results });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching categories with term {Term}", term);
                return Json(new { success = false, message = "An error occurred while searching" });
            }
        }

        [HttpPost]
        [Route("category/toggle-active/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(int id)
        {
            try
            {
                var result = await _categoryService.ToggleCategoryActiveStatusAsync(id);
                if (!result)
                {
                    return NotFound();
                }

                // Clear cache since categories have changed
                _cache.Remove(CategoriesCacheKey);

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling active status for category {CategoryId}", id);
                return Json(new { success = false, message = "An error occurred while updating the category status" });
            }
        }

        private async Task<List<CategoryBreadcrumbViewModel>> GetBreadcrumbsAsync(int categoryId)
        {
            var breadcrumbs = new List<CategoryBreadcrumbViewModel>();
            var currentCategory = await _categoryService.GetByIdAsync(categoryId);
            if (currentCategory == null)
            {
                return breadcrumbs;
            }

            var parentCategories = await _categoryService.GetParentCategoriesAsync(categoryId);

            // Add parent categories to breadcrumbs (from highest to lowest level)
            foreach (var parent in parentCategories.OrderBy(c => c.Level))
            {
                breadcrumbs.Add(new CategoryBreadcrumbViewModel
                {
                    Name = parent.Name,
                    Url = Url.Action("Details", "Category", new { slug = parent.Slug }),
                    IsCurrent = false
                });
            }

            // Add current category
            breadcrumbs.Add(new CategoryBreadcrumbViewModel
            {
                Name = currentCategory.Name,
                Url = Url.Action("Details", "Category", new { slug = currentCategory.Slug }),
                IsCurrent = true
            });

            return breadcrumbs;
        }
    }
}