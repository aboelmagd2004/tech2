using TechXpress.Data.Models;

namespace TechXpress.Web.ViewModels
{
    public class CartItemViewModel
    {
        public string ItemId { get; set; }
        public int Quantity { get; set; }

        public ProductViewModel Product { get; set; }

        //TotalPrice
        public int TotalPrice {  get; set; }
    }
}
