using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace TechXpress.Data.Models
{
    public class ApplicationUser : IdentityUser
    {
        // Add additional properties here
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? ProfilePictureUrl { get; set; } = "/images/download.jpeg"; // default image

        public string? AddressLine1 { get; set; } = "Not Provided";
        public string? AddressLine2 { get; set; } = string.Empty;
        public string? City { get; set; } = "Unknown City";
        public string? State { get; set; } = "Unknown State";
        public string? PostalCode { get; set; } = "00000";
        public string? Country { get; set; } = "Unknown Country";
        public DateTime? AccountCreated { get; set; } = DateTime.UtcNow;
        public DateTime? LastLogin { get; set; } = null;

        // Navigation property for orders
        public virtual ICollection<Order>? Orders { get; set; } = new List<Order>();
        public virtual ShoppingCart? ShoppingCart { get; set; } = new ShoppingCart();

        public virtual ICollection<Review>? Reviews { get; set; } = new List<Review>();

        public virtual ICollection<Payment>? Payments { get; set; } = new List<Payment>();

        // Add any other custom user properties you need



    }
}