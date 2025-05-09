using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechXpress.Data.Models;
using TechXpress.Data.Models.Contexts;
using TechXpress.Data.Repositories.CartItemRepo;
using TechXpress.Data.Repositories.CategoryRepo;
using TechXpress.Data.Repositories.OrderDetailRepo;
using TechXpress.Data.Repositories.OrderRepo;
using TechXpress.Data.Repositories.PaymentRepo;
using TechXpress.Data.Repositories.ProductRepo;
using TechXpress.Data.Repositories.ReviewRepo;
using TechXpress.Data.Repositories.ShoppingCartRepo;

namespace TechXpress.Data.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly TechXpressDbContext _context;
        public TechXpressDbContext Context => _context;

        public IProductRepository Products { get;  set; }
        public ICategoryRepository Categories { get;  set; }
        public IOrderRepository Orders { get;  set; }
        public IOrderDetailRepository OrderDetails { get;  set; }
        public IReviewRepository Reviews { get;  set; }

        public ICartItemRepository CartItems { get; set; }

        public ICartItemRepository cartItems { get; set; }

        public IShoppingCartRepository ShoppingCarts { get; set; }

        public IPaymentRepository Payments { get; set; }

        public UnitOfWork(
            TechXpressDbContext context,
            IProductRepository productRepository,
            ICategoryRepository categoryRepository,
            IOrderRepository orderRepository,
            IOrderDetailRepository orderDetailsRepository,
            ICartItemRepository cartItemRepository,
            IShoppingCartRepository shoppingCartRepository,
            IPaymentRepository paymentRepository,

            IReviewRepository reviewRepository)
        {
            _context = context;
            Products = productRepository;
            Categories = categoryRepository;
            Orders = orderRepository;
            OrderDetails = orderDetailsRepository;
            Reviews = reviewRepository;
            ShoppingCarts = shoppingCartRepository;
            CartItems = cartItemRepository;
            Payments = paymentRepository;
        }

        public async Task<int> CompleteAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
