    using TechXpress.Data.Models;
namespace TechXpress.Web.Extensions
{

    public static class OrderStatusExtensions
    {
        public static string GetStatusBadgeClass(this OrderStatus status)
        {
            switch (status)
            {
                case OrderStatus.Pending:
                    return "badge-secondary"; // Gray for Pending
                case OrderStatus.Processing:
                    return "badge-primary"; // Blue for Processing
                case OrderStatus.Shipped:
                    return "badge-info"; // Light blue for Shipped
                case OrderStatus.Delivered:
                    return "badge-success"; // Green for Delivered
                case OrderStatus.Cancelled:
                    return "badge-danger"; // Red for Cancelled
                case OrderStatus.Returned:
                    return "badge-warning"; // Yellow for Returned
                case OrderStatus.OnHold:
                    return "badge-dark"; // Dark for On Hold
                case OrderStatus.Refunded:
                    return "badge-light"; // Light for Refunded
                default:
                    return "badge-secondary"; // Default gray color for unknown status
            }
        }
    }

}
