using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using TechXpress.Data.Models;
using TechXpress.Data.Repositories.OrderRepo;
using TechXpress.Data.UnitOfWork;
using TechXpress.Services.GenericServices;

namespace TechXpress.Services.OrdersService
{
    public class OrderService : GenericService<Order>, IOrderService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IOrderRepository _orderRepository;
        //_userManager 
        private readonly UserManager<ApplicationUser> _userManager;


        public OrderService(IUnitOfWork unitOfWork,
            UserManager<ApplicationUser> userManager
            )
            : base(unitOfWork.Orders)
        {
            _unitOfWork = unitOfWork;
            _orderRepository = unitOfWork.Orders as IOrderRepository;
            _userManager = userManager;


        }

        public async Task<string> Test()
        {
            var recentOrders = await _orderRepository.GetFilteredOrdersAsync(
                take: 3,
                orderBy: q => q.OrderByDescending(o => o.OrderDate));

            return $"Latest order: {recentOrders.FirstOrDefault()?.OrderNumber} | Order Service is operational";
        }

        //AddOrderAsync
        //public async Task<Order> AddOrderAsync(Order order)
        //{
        //    // Load user entity by ID
        //    var user = await _userManager.FindByIdAsync(order.UserId);

        //    if (user == null)
        //        throw new Exception("User not found");

        //    order.User = user; // assign navigation property

        //    await _unitOfWork.Orders.AddOrderAsync(order);
        //    return order;
        //}

        public async Task<Order> AddOrderAsync(Order order)
        {
            if (string.IsNullOrEmpty(order.UserId))
                throw new ArgumentException("UserId must be set before saving the order.");

            await _unitOfWork.Orders.AddOrderAsync(order);
            return order;
        }

        public async Task<Order> GetOrderWithDetailsAsync(int orderId)
        {
            return await _orderRepository.GetOrderWithDetailsAsync(orderId);
        }

        public async Task<IEnumerable<Order>> GetOrdersByUserAsync(string userId)
        {
            return await _orderRepository.GetOrdersByUserAsync(userId);
        }

        public async Task<IEnumerable<Order>> GetOrdersByStatusAsync(OrderStatus status)
        {
            return await _orderRepository.GetOrdersByStatusAsync(status);
        }

        public async Task<Order> CreateOrderAsync(string userId, AddressViewModel shippingAddress, IEnumerable<OrderDetail> items)
        {
            var order = new Order
            {
                UserId = userId,
                ShippingAddress = shippingAddress,
                OrderDate = DateTime.UtcNow,
                OrderStatus = OrderStatus.Pending,
                PaymentStatus = PaymentStatus.Pending,
                OrderDetails = items.ToList(),
                TotalAmount = items.Sum(i => i.Price * i.Quantity)
            };

            await _unitOfWork.Orders.AddAsync(order);
            await _unitOfWork.CompleteAsync();

            return order;
        }

        public async Task UpdateOrderStatusAsync(int orderId, OrderStatus newStatus, string adminNotes = null)
        {
            await _orderRepository.UpdateOrderStatusAsync(orderId, newStatus, adminNotes);
            await _unitOfWork.CompleteAsync();
        }

        public async Task AddOrderNoteAsync(int orderId, string note, bool isAdminNote = false)
        {
            await _orderRepository.AddOrderNoteAsync(orderId, note, isAdminNote);
            await _unitOfWork.CompleteAsync();
        }

        public async Task<bool> ProcessOrderPaymentAsync(int orderId, string paymentTransactionId)
        {
            var result = await _orderRepository.MarkOrderAsPaidAsync(orderId, paymentTransactionId);
            await _unitOfWork.CompleteAsync();
            return result;
        }

        public async Task<bool> VerifyOrderPaymentAsync(int orderId)
        {
            return await _orderRepository.CheckOrderPaymentStatusAsync(orderId);
        }

        public async Task<decimal> GetSalesTotalAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            return await _orderRepository.GetTotalSalesAsync(startDate, endDate);
        }

        public async Task<int> GetOrderCountAsync(OrderStatus? status = null)
        {
            return await _orderRepository.GetOrderCountAsync(status);
        }

        public async Task<Dictionary<OrderStatus, int>> GetOrderStatusDistributionAsync()
        {
            return await _orderRepository.GetOrderStatusDistributionAsync();
        }

        public async Task<IEnumerable<Order>> GetFilteredOrdersAsync(
            Expression<Func<Order, bool>> filter = null,
            Func<IQueryable<Order>, IOrderedQueryable<Order>> orderBy = null,
            string includeProperties = "",
            int? skip = null,
            int? take = null)
        {
            return await _orderRepository.GetFilteredOrdersAsync(
                filter,
                orderBy,
                includeProperties,
                skip,
                take);
        }

        Task IOrderService.AddOrderAsync(Order order)
        {
            return AddOrderAsync(order);
        }

        //public async Task<Order> CreateOrderAsync(string userId, AddressViewModel shippingAddress, IEnumerable<OrderDetail> items)
        //{
        //    // 1. Validate input (you can add custom validation here)
        //    if (string.IsNullOrEmpty(userId))
        //    {
        //        throw new ArgumentException("UserId is required");
        //    }

        //    if (items == null || !items.Any())
        //    {
        //        throw new ArgumentException("Order must contain at least one item");
        //    }

        //    // 2. Calculate the total price, tax, and shipping (as per your logic)
        //    decimal subtotal = items.Sum(item => item.Price * item.Quantity);
        //    decimal taxAmount = subtotal * 0.1m; // Example: 10% tax
        //    decimal shippingFee = subtotal > 50 ? 0m : 5.99m; // Free shipping for orders above $50
        //    decimal totalAmount = subtotal + taxAmount + shippingFee;

        //    // 3. Create the order object
        //    var order = new Order
        //    {
        //        UserId = userId,
        //        ShippingAddress = new AddressViewModel
        //        {
        //            // Assuming shippingAddress is a JSON string that can be deserialized into AddressViewModel
        //            FirstName = shippingAddress.FirstName,
        //            LastName = shippingAddress.LastName,
        //            AddressLine1 = shippingAddress.AddressLine1,
        //            AddressLine2 = shippingAddress.AddressLine2,
        //            City = shippingAddress.City,
        //            State = shippingAddress.State,
        //            ZipCode = shippingAddress.ZipCode,
        //            Country = shippingAddress.Country,
        //            PhoneNumber = shippingAddress.PhoneNumber
        //        },
        //        OrderDetails = items.ToList(),
        //        TotalAmount = totalAmount,
        //        OrderStatus = OrderStatus.Pending, // Set initial status to Pending
        //        OrderDate = DateTime.UtcNow,
        //    };

        //    // 4. Use the _orderRepository to save the order to the database
        //    await _orderRepository.AddAsync(order);

        //    // 5. Return the created order
        //    return order;
        //}

    }
}