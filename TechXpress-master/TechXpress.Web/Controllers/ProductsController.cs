using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Expressions;
using System.Security.Claims;
using TechXpress.Data.Models;
using TechXpress.Services;
using TechXpress.Services.CategoriesService;
using TechXpress.Services.ProductsService;
using TechXpress.Services.ReviewsService;
using TechXpress.Services.ShoppingCartsService;
using TechXpress.Web.ViewModels;
using TechXpress.Web.ViewModels.Products;
using X.PagedList;
using X.PagedList.Extensions;

namespace TechXpress.Web.Controllers
{
    public class ProductsController : BaseController
    {
        private readonly ICategoryService _categoryService;
        private readonly IReviewService _reviewService;
        private const int PageSize = 9;

        public ProductsController(
            IProductService productService,
            IShoppingCartService cartService,
            ICategoryService categoryService,
            IReviewService reviewService)
            : base(productService, cartService)
        {
            _categoryService = categoryService;
            _reviewService = reviewService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(
            int? page,
            int? categoryId,
            string searchTerm,
            string sortOrder,
            decimal? minPrice,
            decimal? maxPrice)
        {
            int pageNumber = page ?? 1;

            // Build the filter expression
            Expression<Func<Data.Models.ProductViewModel, bool>> filter = p =>
                (categoryId == null || p.CategoryId == categoryId) &&
                (string.IsNullOrEmpty(searchTerm) ||
                 p.Name.Contains(searchTerm) ||
                 p.Description.Contains(searchTerm)) &&
                (!minPrice.HasValue || p.Price >= minPrice.Value) &&
                (!maxPrice.HasValue || p.Price <= maxPrice.Value);

            // Determine sorting
            Func<IQueryable<Data.Models.ProductViewModel>, IOrderedQueryable<Data.Models.ProductViewModel>> orderBy = null;
            switch (sortOrder)
            {
                case "price_asc":
                    orderBy = q => q.OrderBy(p => p.Price);
                    break;
                case "price_desc":
                    orderBy = q => q.OrderByDescending(p => p.Price);
                    break;
                case "newest":
                    orderBy = q => q.OrderByDescending(p => p.CreatedDate);
                    break;
                case "rating":
                    orderBy = q => q.OrderByDescending(p => p.Reviews.Average(r => r.Rating));
                    break;
                default:
                    orderBy = q => q.OrderBy(p => p.Name);
                    break;
            }

            // Get filtered and sorted products
            var products = await _productService.GetFilteredAsync(
                filter: filter,
                orderBy: orderBy,
                includeProperties: "Category,Reviews");

            // Get recommended products (top rated in same category)
            var recommendedProducts = categoryId.HasValue
                ? await _productService.GetFilteredAsync(
                    filter: p => p.CategoryId == categoryId && p.Id != categoryId,
                    orderBy: q => q.OrderByDescending(p => p.Reviews.Average(r => r.Rating)),
                    take: 2)
                : Enumerable.Empty<Data.Models.ProductViewModel>();

            var viewModel = new ProductIndexViewModel
            {
                Products = products.ToPagedList(pageNumber, PageSize),
                Categories = await _categoryService.GetAllAsync(),
                SelectedCategoryId = categoryId,
                SearchTerm = searchTerm,
                SortOrder = sortOrder,
                MinPrice = minPrice,
                MaxPrice = maxPrice,
                RecommendedProducts = recommendedProducts,
                CartItemCount = await GetCartItemCountAsync()
            };

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var product = await _productService.GetProductsWithCategoryAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            var similarProducts = await _productService.GetFilteredAsync(
                filter: p => p.CategoryId == product.CategoryId && p.Id != product.Id,
                take: 4);

            var reviews = await _reviewService.GetFilteredAsync(
                filter: r => r.ProductId == id,
                includeProperties: "User");

            

            var viewModel = new ProductDetailsViewModel
            {
                Product = product,
                SimilarProducts = similarProducts,
                Reviews = reviews.Select(r => new ReviewViewModel
                {
                    Id = r.Id,
                    ProductId = r.ProductId,
                    Product = r.Product,
                    UserId = r.UserId,
                    User = r.User,
                    Rating = r.Rating,
                    Comment = r.Comment,
                    CreatedAt = r.CreatedAt,
                    UpdatedAt = r.UpdatedAt
                }).ToList(), // Materialize the collection with ToList()
                AverageRating = reviews.Any() ? reviews.Average(r => r.Rating) : 0,
                ReviewCount = reviews.Count(),
                CartItemCount = await GetCartItemCountAsync(),
                NewReview = new ReviewViewModel
                {
                    ProductId = id, // Use the id parameter directly
                    UserId = userId,
                    Rating = 0,
                    Comment = string.Empty,
                    CreatedAt = DateTime.UtcNow
                    // No need to set UpdatedAt or User or Product properties
                }
            };

            return View(viewModel); // No need to specify "Details" as it's the default view name
        }
        [HttpGet]
        public async Task<IActionResult> QuickView(int id)
        {
            try
            {
                var product = await _productService.GetProductsWithCategoryAsync(id);
                if (product == null)
                {
                    return NotFound();
                }
                return PartialView("_ProductQuickView", product);
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error in QuickView: {ex.Message}");
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        [HttpGet]
        public async Task<IActionResult> SearchSuggestions(string term)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(term) || term.Length < 2)
                {
                    return Json(new { success = false, message = "Minimum 2 characters required" });
                }

                var suggestions = await _productService.GetFilteredAsync(
                    filter: p => p.Name.Contains(term) || (p.Category != null && p.Category.Name.Contains(term)),
                    take: 5);

                return Json(new
                {
                    success = true,
                    results = suggestions.Select(p => new
                    {
                        id = p.Id,
                        name = p.Name,
                        category = p.Category?.Name ?? "Uncategorized",
                        image = p.ImageUrl ?? "/images/default-product.png"
                    })
                });
            }
            catch (Exception ex)
            {
                //_logger.LogError(ex, "Error in SearchSuggestions");
                return StatusCode(500, new { success = false, message = "An error occurred" });
            }
        }





        [HttpPost("AddReview")]
        public async Task<IActionResult> AddReview(ReviewViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Check if the model is correctly populated
                if (model == null)
                {
                    // Log or throw an error to understand why it's null
                    return BadRequest("Model is null.");
                }

                var review = new Review
                {
                    ProductId = model.ProductId,
                    UserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                    Rating = model.Rating,
                    Comment = model.Comment,
                    CreatedAt = DateTime.UtcNow
                };

                await _reviewService.AddAsync(review);
                await _reviewService.SaveAsync();

                TempData["SuccessMessage"] = "Thank you for your review!";
                return RedirectToAction("Details", new { id = model.ProductId });
            }

            return await Details(model.ProductId);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> DeleteReview(int id, int productId)
        {
            var review = await _reviewService.GetByIdAsync(id);
            if (review == null || review.UserId != User.FindFirstValue(ClaimTypes.NameIdentifier))
            {
                return NotFound();
            }

            await _reviewService.DeleteAsync(review);
            await _reviewService.SaveAsync();

            TempData["SuccessMessage"] = "Your review has been deleted.";
            return RedirectToAction("Details", new { id = productId });
        }
    }
}