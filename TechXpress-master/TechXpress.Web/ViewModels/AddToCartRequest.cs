using TechXpress.Data.Models;

namespace TechXpress.Web.ViewModels
{
    public class AddToCartRequest
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; } = 1;
    }

    public class UpdateCartItemRequest
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }

    public class RemoveCartItemRequest
    {
        public int ProductId { get; set; }
    }

    public class CartSummaryViewModel
    {
        public int ItemCount { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Tax { get; set; }
        public decimal Shipping { get; set; }
        public decimal Total { get; set; }
        public List<CartSummaryItemViewModel> Items { get; set; } = new List<CartSummaryItemViewModel>();
    }

    public class CartSummaryItemViewModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public string ImageUrl { get; set; }
    }

    public class CheckoutViewModel
    {
        public CartViewModel? Cart { get; set; }
        public AddressViewModel ShippingAddress { get; set; }
        public AddressViewModel BillingAddress { get; set; }
        public string PaymentMethod { get; set; }
        public bool SameAsShipping { get; set; } = true;
    }

    //public class AddressViewModel
    //{
    //    public string FirstName { get; set; }
    //    public string LastName { get; set; }
    //    public string AddressLine1 { get; set; }
    //    public string AddressLine2 { get; set; }
    //    public string City { get; set; }
    //    public string State { get; set; }
    //    public string ZipCode { get; set; }
    //    public string Country { get; set; } = "US";
    //    public string PhoneNumber { get; set; }
    //}
}
