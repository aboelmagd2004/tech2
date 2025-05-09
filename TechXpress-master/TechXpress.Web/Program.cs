using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StripeOrg=Stripe;
using TechXpress.Data.Models;
using TechXpress.Data.Models.Contexts;
using TechXpress.Data.Repositories.CartItemRepo;
using TechXpress.Data.Repositories.CategoryRepo;
using TechXpress.Data.Repositories.GenericRepository;
using TechXpress.Data.Repositories.OrderDetailRepo;
using TechXpress.Data.Repositories.OrderRepo;
using TechXpress.Data.Repositories.ProductRepo;
using TechXpress.Data.Repositories.ReviewRepo;
using TechXpress.Data.Repositories.ShoppingCartRepo;
using TechXpress.Data.UnitOfWork;
using TechXpress.Services.CartItemsService;
using TechXpress.Services.CategoriesService;
using TechXpress.Services.OrdersService;
using TechXpress.Services.ProductsService;
using TechXpress.Services.ReviewsService;
using TechXpress.Services.OrdersDetailsService;
using TechXpress.Services.ShoppingCartsService;
using TechXpress.Web.Filters;
using TechXpress.Web.Services.Interfaces;
using TechXpress.Web.Services.Implementations;
using TechXpress.Web.Areas.Admin.Services;
using TechXpress.Services.Payment;
using TechXpress.Data.Repositories.PaymentRepo;
using TechXpress.Services.CustomersService;
using TechXpress.Services.GenericServices;

namespace TechXpress.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Load configuration from appsettings.json
            builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            // Configure the DbContext with migrations assembly
            builder.Services.AddDbContext<TechXpressDbContext>(options =>
                options.UseSqlServer(
                    builder.Configuration.GetConnectionString("DefaultConnection"),
                    b => b.MigrationsAssembly("TechXpress.Data")));

            // Configure Identity with ApplicationUser
            builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 8;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;
                options.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<TechXpressDbContext>()
            .AddDefaultTokenProviders();



            // Add repositories for data access
            builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            builder.Services.AddScoped<IProductRepository, ProductRepository>();
            builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
            builder.Services.AddScoped<IOrderRepository, OrderRepository>();
            builder.Services.AddScoped<IReviewRepository, ReviewRepository>();
            builder.Services.AddScoped<IOrderDetailRepository, OrderDetailRepository>();
            builder.Services.AddScoped<IShoppingCartRepository, ShoppingCartRepository>();
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
            builder.Services.AddScoped<ICartItemRepository, CartItemRepository>();


            // Add services for business logic
            builder.Services.AddScoped(typeof(IGenericService<>), typeof(GenericService<>));
            builder.Services.AddScoped<IProductService, ProductService>();
            builder.Services.AddScoped<ICartItemService, CartItemService>();
            builder.Services.AddScoped<IOrderService, OrderService>();
            builder.Services.AddScoped<IReviewService, ReviewService>();
            builder.Services.AddScoped<ICategoryService, CategoryService>();
            builder.Services.AddScoped<IOrderDetailsService, OrderDetailsService>();
            builder.Services.AddScoped<IShoppingCartService, ShoppingCartService>();
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<ICustomerService, CustomerService>(); // Register the service
            builder.Services.AddScoped<Areas.Admin.Services.IImageService, Areas.Admin.Services.ImageService>();



            // Add AI Services (Ensure these interfaces and implementations are defined correctly)
            builder.Services.AddHttpClient();
            builder.Services.AddTransient<IAIAssistantService, AIAssistantService>();
            builder.Services.AddTransient<IAICommerceService, AICommerceAssistantService>();



            // Add Stripe settings
            // Configure Stripe settings and register StripeSettings instance
            var stripeSettingsSection = builder.Configuration.GetSection("Stripe");
            builder.Services.Configure<StripeSettings>(stripeSettingsSection);
            builder.Services.AddSingleton(stripeSettingsSection.Get<StripeSettings>());

            builder.Services.AddScoped<IStripeService, StripeService>();
            builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();


            // Email Sender Service
            builder.Services.AddTransient<IEmailSender, EmailSender>();

            // Add Filters (For example, UpdateCartItemCountFilter)
            builder.Services.AddScoped<UpdateCartItemCountFilter>();

            //Admin Area Services
            builder.Services.AddScoped<IAdminDashboardService, AdminDashboardService>();

            var app = builder.Build();

            var stripeSettings = app.Services.GetRequiredService<StripeSettings>();
            StripeOrg.StripeConfiguration.ApiKey = stripeSettings.SecretKey;

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            // Add response caching services
            builder.Services.AddResponseCaching();
            app.UseResponseCaching();

            // Configure endpoint routing for Areas (Admin section)
            app.UseEndpoints(endpoints =>
            {
                // Areas routing with default controller action
                endpoints.MapControllerRoute(
                    name: "areas",
                    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}"
                );
            });

            // Default route (home page)
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
