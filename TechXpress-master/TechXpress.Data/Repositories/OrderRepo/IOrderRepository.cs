using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using TechXpress.Data.Models;
using TechXpress.Data.Repositories.GenericRepository;

namespace TechXpress.Data.Repositories.OrderRepo
{
    public interface IOrderRepository : IGenericRepository<Order>
    {
        // Basic test method
        string TestRepository();

        // Core order operations
        Task<Order> GetOrderWithDetailsAsync(int orderId);
        //AddOrderAsync 
        Task AddOrderAsync(Order order);

        Task<Order> CreateOrderAsync(string userId, AddressViewModel shippingAddress, IEnumerable<OrderDetail> items);
        Task<IEnumerable<Order>> GetOrdersByUserAsync(string userId);
        Task<IEnumerable<Order>> GetOrdersByStatusAsync(OrderStatus status);

        // Order statistics
        Task<decimal> GetTotalSalesAsync(DateTime? startDate = null, DateTime? endDate = null);
        Task<int> GetOrderCountAsync(OrderStatus? status = null);
        Task<Dictionary<OrderStatus, int>> GetOrderStatusDistributionAsync();

        // Advanced filtering
        Task<IEnumerable<Order>> GetFilteredOrdersAsync(
            Expression<Func<Order, bool>> filter = null,
            Func<IQueryable<Order>, IOrderedQueryable<Order>> orderBy = null,
            string includeProperties = "",
            int? skip = null,
            int? take = null);

        // Order management
        Task UpdateOrderStatusAsync(int orderId, OrderStatus newStatus, string adminNotes = null);
        Task AddOrderNoteAsync(int orderId, string note, bool isAdminNote = false);

        // Payment related
        Task<bool> MarkOrderAsPaidAsync(int orderId, string paymentTransactionId);
        Task<bool> CheckOrderPaymentStatusAsync(int orderId);
    }
}