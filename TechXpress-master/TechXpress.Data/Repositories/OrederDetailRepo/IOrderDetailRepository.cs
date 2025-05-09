using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using TechXpress.Data.Models;
using TechXpress.Data.Repositories.GenericRepository;

namespace TechXpress.Data.Repositories.OrderDetailRepo
{
    public interface IOrderDetailRepository : IGenericRepository<OrderDetail>
    {
        // Basic test method
        string TestRepository();

        // Core order detail operations
        Task<IEnumerable<OrderDetail>> GetDetailsForOrderAsync(int orderId);
        Task<IEnumerable<OrderDetail>> GetDetailsForProductAsync(int productId);
        Task<OrderDetail> GetDetailWithOrderAndProductAsync(int orderDetailId);

        // Order detail statistics
        Task<decimal> GetTotalRevenueForProductAsync(int productId);
        Task<int> GetTotalSoldForProductAsync(int productId);
        Task<Dictionary<int, int>> GetProductSalesDistributionAsync(int orderId);

        // Batch operations
        Task AddRangeAsync(IEnumerable<OrderDetail> orderDetails);
        Task UpdateRangeAsync(IEnumerable<OrderDetail> orderDetails);

        // Advanced filtering
        Task<IEnumerable<OrderDetail>> GetFilteredOrderDetailsAsync(
            Expression<Func<OrderDetail, bool>> filter = null,
            Func<IQueryable<OrderDetail>, IOrderedQueryable<OrderDetail>> orderBy = null,
            string includeProperties = "",
            int? skip = null,
            int? take = null);
    }
}