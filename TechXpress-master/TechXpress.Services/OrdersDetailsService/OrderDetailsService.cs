using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using TechXpress.Data.Models;
using TechXpress.Data.Repositories.OrderDetailRepo;
using TechXpress.Data.UnitOfWork;
using TechXpress.Services.GenericServices;

namespace TechXpress.Services.OrdersDetailsService
{
    public class OrderDetailsService : GenericService<OrderDetail>, IOrderDetailsService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IOrderDetailRepository _orderDetailRepository;

        public OrderDetailsService(IUnitOfWork unitOfWork)
            : base(unitOfWork.OrderDetails)
        {
            _unitOfWork = unitOfWork;
            _orderDetailRepository = unitOfWork.OrderDetails as IOrderDetailRepository;
        }

        public async Task<string> Test()
        {
            var sampleDetails = await _orderDetailRepository.GetFilteredOrderDetailsAsync(take: 3);
            return $"Sample order details loaded | OrderDetails Service is operational (Sample Order ID: {sampleDetails.FirstOrDefault()?.OrderId})";
        }

        public async Task<IEnumerable<OrderDetail>> GetDetailsForOrderAsync(int orderId)
        {
            return await _orderDetailRepository.GetDetailsForOrderAsync(orderId);
        }

        public async Task<IEnumerable<OrderDetail>> GetDetailsForProductAsync(int productId)
        {
            return await _orderDetailRepository.GetDetailsForProductAsync(productId);
        }

        public async Task<OrderDetail> GetDetailWithOrderAndProductAsync(int orderDetailId)
        {
            return await _orderDetailRepository.GetDetailWithOrderAndProductAsync(orderDetailId);
        }

        public async Task AddRangeAsync(IEnumerable<OrderDetail> orderDetails)
        {
            await _orderDetailRepository.AddRangeAsync(orderDetails);
            await _unitOfWork.CompleteAsync();
        }

        public async Task UpdateRangeAsync(IEnumerable<OrderDetail> orderDetails)
        {
            await _orderDetailRepository.UpdateRangeAsync(orderDetails);
            await _unitOfWork.CompleteAsync();
        }

        public async Task<decimal> GetTotalRevenueForProductAsync(int productId)
        {
            return await _orderDetailRepository.GetTotalRevenueForProductAsync(productId);
        }

        public async Task<int> GetTotalSoldForProductAsync(int productId)
        {
            return await _orderDetailRepository.GetTotalSoldForProductAsync(productId);
        }

        public async Task<Dictionary<int, int>> GetProductSalesDistributionAsync(int orderId)
        {
            return await _orderDetailRepository.GetProductSalesDistributionAsync(orderId);
        }

        public async Task<IEnumerable<OrderDetail>> GetFilteredOrderDetailsAsync(
            Expression<Func<OrderDetail, bool>> filter = null,
            Func<IQueryable<OrderDetail>, IOrderedQueryable<OrderDetail>> orderBy = null,
            string includeProperties = "",
            int? skip = null,
            int? take = null)
        {
            return await _orderDetailRepository.GetFilteredOrderDetailsAsync(
                filter,
                orderBy,
                includeProperties,
                skip,
                take);
        }
    }
}