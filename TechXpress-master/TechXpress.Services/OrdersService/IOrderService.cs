using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using TechXpress.Data.Models;
using TechXpress.Services.GenericServices;

namespace TechXpress.Services.OrdersService
{
    public interface IOrderService : IGenericService<Order>
    {
        Task<string> Test();

        // Order-specific operations
        Task<Order> GetOrderWithDetailsAsync(int orderId);
        Task<IEnumerable<Order>> GetOrdersByUserAsync(string userId);
        Task<IEnumerable<Order>> GetOrdersByStatusAsync(OrderStatus status);

        // Order processing
        Task<Order> CreateOrderAsync(string userId, AddressViewModel shippingAddress, IEnumerable<OrderDetail> items);
        //AddOrderAsync
        Task AddOrderAsync(Order order);

        Task UpdateOrderStatusAsync(int orderId, OrderStatus newStatus, string adminNotes = null);
        Task AddOrderNoteAsync(int orderId, string note, bool isAdminNote = false);

        // Payment handling
        Task<bool> ProcessOrderPaymentAsync(int orderId, string paymentTransactionId);
        Task<bool> VerifyOrderPaymentAsync(int orderId);

        // Reporting
        Task<decimal> GetSalesTotalAsync(DateTime? startDate = null, DateTime? endDate = null);
        Task<int> GetOrderCountAsync(OrderStatus? status = null);
        Task<Dictionary<OrderStatus, int>> GetOrderStatusDistributionAsync();

        // Advanced filtering
        Task<IEnumerable<Order>> GetFilteredOrdersAsync(
            Expression<Func<Order, bool>> filter = null,
            Func<IQueryable<Order>, IOrderedQueryable<Order>> orderBy = null,
            string includeProperties = "",
            int? skip = null,
            int? take = null);
    }
}