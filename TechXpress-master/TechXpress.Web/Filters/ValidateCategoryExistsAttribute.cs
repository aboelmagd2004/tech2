// TechXpress.Web/Filters/ValidateCategoryExistsAttribute.cs
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using TechXpress.Services.CategoriesService;

public class ValidateCategoryExistsAttribute : TypeFilterAttribute
{
    public ValidateCategoryExistsAttribute() : base(typeof(ValidateCategoryExistsFilter))
    {
    }

    private class ValidateCategoryExistsFilter : IAsyncActionFilter
    {
        private readonly ICategoryService _categoryService;
        private readonly ILogger<ValidateCategoryExistsFilter> _logger;

        public ValidateCategoryExistsFilter(ICategoryService categoryService, ILogger<ValidateCategoryExistsFilter> logger)
        {
            _categoryService = categoryService;
            _logger = logger;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var id = 0;
            if (context.ActionArguments.ContainsKey("id"))
            {
                id = (int)context.ActionArguments["id"];
            }
            else if (context.RouteData.Values.ContainsKey("id"))
            {
                id = int.Parse(context.RouteData.Values["id"].ToString());
            }

            if (id == 0)
            {
                context.Result = new NotFoundResult();
                return;
            }

            var category = await _categoryService.GetByIdAsync(id);
            if (category == null)
            {
                _logger.LogWarning("Category with ID {CategoryId} not found", id);
                context.Result = new NotFoundResult();
                return;
            }

            context.HttpContext.Items["category"] = category;
            await next();
        }
    }
}

// TechXpress.Web/Attributes/AllowedExtensionsAttribute.cs
public class AllowedExtensionsAttribute : ValidationAttribute
{
    private readonly string[] _extensions;

    public AllowedExtensionsAttribute(string[] extensions)
    {
        _extensions = extensions;
    }

    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        if (value is IFormFile file)
        {
            var extension = Path.GetExtension(file.FileName);
            if (!_extensions.Contains(extension.ToLower()))
            {
                return new ValidationResult(GetErrorMessage());
            }
        }
        return ValidationResult.Success;
    }

    public string GetErrorMessage()
    {
        return $"Only the following file types are allowed: {string.Join(", ", _extensions)}";
    }
}

// TechXpress.Web/Attributes/MaxFileSizeAttribute.cs
public class MaxFileSizeAttribute : ValidationAttribute
{
    private readonly int _maxFileSize;

    public MaxFileSizeAttribute(int maxFileSize)
    {
        _maxFileSize = maxFileSize;
    }

    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        if (value is IFormFile file)
        {
            if (file.Length > _maxFileSize)
            {
                return new ValidationResult(GetErrorMessage());
            }
        }
        return ValidationResult.Success;
    }

    public string GetErrorMessage()
    {
        return $"Maximum allowed file size is {_maxFileSize / 1024 / 1024}MB";
    }
}