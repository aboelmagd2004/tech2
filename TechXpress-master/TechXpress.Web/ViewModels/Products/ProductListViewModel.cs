using System.Collections.Generic;
using TechXpress.Data.Models;

namespace TechXpress.Web.ViewModels.Products
{
    public class ProductListViewModel
    {
        public IEnumerable<Data.Models.ProductViewModel> Products { get; set; }
        public IEnumerable<Category> Categories { get; set; }
        public int CartItemCount { get; set; }
        public int? SelectedCategoryId { get; set; }
    }
}