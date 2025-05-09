using System.Collections.Generic;
using TechXpress.Data.Models;

namespace TechXpress.Web.ViewModels.Products
{
    public class ProductDetailsViewModel
    {
        public Data.Models.ProductViewModel Product { get; set; }
        public IEnumerable<Data.Models.ProductViewModel> SimilarProducts { get; set; }
        public IEnumerable<ReviewViewModel> Reviews { get; set; }
        public double AverageRating { get; set; }
        public int ReviewCount { get; set; }
        public int CartItemCount { get; set; }
        public ReviewViewModel NewReview { get; set; } = new ReviewViewModel();
    }
}