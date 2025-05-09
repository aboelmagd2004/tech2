// TechXpress.Web/ViewModels/Category/CategoryViewModel.cs
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using TechXpress.Data.Models;

public class CategoryViewModel
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; }

    public string Slug { get; set; }

    [StringLength(500)]
    public string Description { get; set; }

    public string ImageUrl { get; set; }

    public int? ParentId { get; set; }

    public int DisplayOrder { get; set; }

    public bool IsActive { get; set; }

    [StringLength(70)]
    public string MetaTitle { get; set; }

    [StringLength(160)]
    public string MetaDescription { get; set; }

    [StringLength(500)]
    public string MetaKeywords { get; set; }

    public int ProductCount { get; set; }
}

// TechXpress.Web/ViewModels/Category/CategoryDetailsViewModel.cs
public class CategoryDetailsViewModel : CategoryViewModel
{
    public PaginatedList<ProductViewModel> Products { get; set; }
    public List<CategoryViewModel> ParentCategories { get; set; }
    public List<CategoryViewModel> ChildCategories { get; set; }
    public List<CategoryViewModel> RelatedCategories { get; set; }
}

// TechXpress.Web/ViewModels/Category/CategoryCreateViewModel.cs
public class CategoryCreateViewModel
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; }

    [StringLength(500)]
    public string Description { get; set; }

    [Display(Name = "Parent Category")]
    public int? ParentId { get; set; }

    [Display(Name = "Display Order")]
    public int DisplayOrder { get; set; }

    [Display(Name = "Category Image")]
    [AllowedExtensions(new string[] { ".jpg", ".jpeg", ".png", ".gif" })]
    [MaxFileSize(5 * 1024 * 1024)]
    public IFormFile ImageFile { get; set; }

    [StringLength(70)]
    public string MetaTitle { get; set; }

    [StringLength(160)]
    public string MetaDescription { get; set; }

    [StringLength(500)]
    public string MetaKeywords { get; set; }

    public SelectList AvailableParentCategories { get; set; }
}

// TechXpress.Web/ViewModels/Category/CategoryEditViewModel.cs
public class CategoryEditViewModel : CategoryCreateViewModel
{
    public int Id { get; set; }
    public string ExistingImageUrl { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime ModifiedDate { get; set; }
}

// TechXpress.Web/ViewModels/Category/CategoryListViewModel.cs
public class CategoryListViewModel
{
    public PaginatedList<CategoryViewModel> Categories { get; set; }
    public int TotalCount { get; set; }
    public int ActiveCount { get; set; }
    public int InactiveCount { get; set; }
}

// TechXpress.Web/ViewModels/Category/CategoryBreadcrumbViewModel.cs
public class CategoryBreadcrumbViewModel
{
    public string Name { get; set; }
    public string Url { get; set; }
    public bool IsCurrent { get; set; }
}





public class PaginatedList<T> : List<T>
{
    public int PageIndex { get; private set; }
    public int TotalPages { get; private set; }

    public PaginatedList(List<T> items, int count, int pageIndex, int pageSize)
    {
        PageIndex = pageIndex;
        TotalPages = (int)Math.Ceiling(count / (double)pageSize);
        this.AddRange(items);
    }

    public bool HasPreviousPage => PageIndex > 1;
    public bool HasNextPage => PageIndex < TotalPages;

    public int PageSize { get; private set; }

    // For IQueryable (EF Core)
    public static async Task<PaginatedList<T>> CreateAsync(IQueryable<T> source, int pageIndex, int pageSize)
    {
        var count = await source.CountAsync();
        var items = await source.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();
        return new PaginatedList<T>(items, count, pageIndex, pageSize);
    }

    // For List<T> or IEnumerable<T>
    public static PaginatedList<T> Create(IEnumerable<T> source, int pageIndex, int pageSize)
    {
        var list = source.ToList();
        var count = list.Count;
        var items = list.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
        return new PaginatedList<T>(items, count, pageIndex, pageSize);
    }

   
    public static PaginatedList<T> ConvertToProductViewModelPaginatedList(PaginatedList<TechXpress.Web.ViewModels.Products.ProductViewModel> source)
{
    // Create a new PaginatedList<T> based on the list in the ProductViewModel
    var items = source.Select(item => (T)(object)item).ToList(); // Adjust based on the real type T
    return new PaginatedList<T>(items, source.Count, source.PageIndex, source.PageSize);
}

    internal static PaginatedList<ProductViewModel> Create(IEnumerable<ProductViewModel> productViewModels, int page, int pageSize)
    {
        // Ensure valid pagination parameters
        page = Math.Max(1, page);
        pageSize = Math.Max(1, Math.Min(50, pageSize)); // Limit page size to reasonable values

        // Count total number of items
        var count = productViewModels.Count();

        // Calculate total pages
        var totalPages = (int)Math.Ceiling(count / (double)pageSize);

        // Select the items for the current page
        var items = productViewModels
            .Skip((page - 1) * pageSize) // Skip items for previous pages
            .Take(pageSize) // Take the items for the current page
            .ToList();

        return new PaginatedList<ProductViewModel>(items, count, page, pageSize);
    }


    public static implicit operator PaginatedList<T>(PaginatedList<TechXpress.Web.ViewModels.Products.ProductViewModel> v)
    {
        throw new NotImplementedException();
    }
}
