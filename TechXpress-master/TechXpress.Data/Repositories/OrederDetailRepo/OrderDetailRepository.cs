using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TechXpress.Data.Models;
using TechXpress.Data.Models.Contexts;
using TechXpress.Data.Repositories.GenericRepository;

namespace TechXpress.Data.Repositories.OrderDetailRepo
{
    public class OrderDetailRepository : GenericRepository<OrderDetail>, IOrderDetailRepository
    {
        public OrderDetailRepository(TechXpressDbContext context) : base(context)
        {
        }

        public string TestRepository()
        {
            return "Order Detail Repository is operational";
        }

        public async Task<IEnumerable<OrderDetail>> GetDetailsForOrderAsync(int orderId)
        {
            return await _context.Set<OrderDetail>()
                .Where(od => od.OrderId == orderId)
                .Include(od => od.Product)
                .OrderBy(od => od.OrderDetailId)
                .ToListAsync();
        }

        public async Task<IEnumerable<OrderDetail>> GetDetailsForProductAsync(int productId)
        {
            return await _context.Set<OrderDetail>()
                .Where(od => od.ProductId == productId)
                .Include(od => od.Order)
                .OrderByDescending(od => od.Order.OrderDate)
                .ToListAsync();
        }

        public async Task<OrderDetail> GetDetailWithOrderAndProductAsync(int orderDetailId)
        {
            return await _context.Set<OrderDetail>()
                .Include(od => od.Order)
                .Include(od => od.Product)
                .FirstOrDefaultAsync(od => od.OrderDetailId == orderDetailId);
        }

        public async Task<decimal> GetTotalRevenueForProductAsync(int productId)
        {
            return await _context.Set<OrderDetail>()
                .Where(od => od.ProductId == productId)
                .SumAsync(od => od.Price * od.Quantity);
        }

        public async Task<int> GetTotalSoldForProductAsync(int productId)
        {
            return await _context.Set<OrderDetail>()
                .Where(od => od.ProductId == productId)
                .SumAsync(od => od.Quantity);
        }

        public async Task<Dictionary<int, int>> GetProductSalesDistributionAsync(int orderId)
        {
            return await _context.Set<OrderDetail>()
                .Where(od => od.OrderId == orderId)
                .GroupBy(od => od.ProductId)
                .Select(g => new { ProductId = g.Key, Quantity = g.Sum(od => od.Quantity) })
                .ToDictionaryAsync(x => x.ProductId, x => x.Quantity);
        }

        public async Task AddRangeAsync(IEnumerable<OrderDetail> orderDetails)
        {
            await _context.Set<OrderDetail>().AddRangeAsync(orderDetails);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateRangeAsync(IEnumerable<OrderDetail> orderDetails)
        {
            _context.Set<OrderDetail>().UpdateRange(orderDetails);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<OrderDetail>> GetFilteredOrderDetailsAsync(
            Expression<Func<OrderDetail, bool>> filter = null,
            Func<IQueryable<OrderDetail>, IOrderedQueryable<OrderDetail>> orderBy = null,
            string includeProperties = "",
            int? skip = null,
            int? take = null)
        {
            IQueryable<OrderDetail> query = _context.Set<OrderDetail>();

            if (filter != null)
            {
                query = query.Where(filter);
            }

            foreach (var includeProperty in includeProperties.Split(
                new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                query = query.Include(includeProperty);
            }

            if (orderBy != null)
            {
                query = orderBy(query);
            }

            if (skip.HasValue)
            {
                query = query.Skip(skip.Value);
            }

            if (take.HasValue)
            {
                query = query.Take(take.Value);
            }

            return await query.AsNoTracking().ToListAsync();
        }
    }
}