using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using TechXpress.Data.Models;
using TechXpress.Services.GenericServices;

namespace TechXpress.Services.OrdersDetailsService
{
    public interface IOrderDetailsService : IGenericService<OrderDetail>
    {
        Task<string> Test();

        // OrderDetail-specific operations
        Task<IEnumerable<OrderDetail>> GetDetailsForOrderAsync(int orderId);
        Task<IEnumerable<OrderDetail>> GetDetailsForProductAsync(int productId);
        Task<OrderDetail> GetDetailWithOrderAndProductAsync(int orderDetailId);

        // Batch operations
        Task AddRangeAsync(IEnumerable<OrderDetail> orderDetails);
        Task UpdateRangeAsync(IEnumerable<OrderDetail> orderDetails);

        // Sales analytics
        Task<decimal> GetTotalRevenueForProductAsync(int productId);
        Task<int> GetTotalSoldForProductAsync(int productId);
        Task<Dictionary<int, int>> GetProductSalesDistributionAsync(int orderId);

        // Advanced filtering
        Task<IEnumerable<OrderDetail>> GetFilteredOrderDetailsAsync(
            Expression<Func<OrderDetail, bool>> filter = null,
            Func<IQueryable<OrderDetail>, IOrderedQueryable<OrderDetail>> orderBy = null,
            string includeProperties = "",
            int? skip = null,
            int? take = null);
    }
}