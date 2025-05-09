using System;
using System.Collections.Generic;
using System.Linq;
using TechXpress.Web.Areas.Admin.Models;
using TechXpress.Data;
using TechXpress.Data.Models;
using TechXpress.Web.Areas.Admin.Models;
using TechXpress.Data.Models.Contexts;

namespace TechXpress.Web.Areas.Admin.Services
{
   

    public class AdminDashboardService : IAdminDashboardService
    {
        private readonly TechXpressDbContext _context;

        public AdminDashboardService(TechXpressDbContext context)
        {
            _context = context;
        }

        public DashboardViewModel GetDashboardData()
        {
            var now = DateTime.UtcNow;
            var startOfMonth = new DateTime(now.Year, now.Month, 1);

            return new DashboardViewModel
            {
                TotalUsers = _context.Users.Count(),
                NewUsersThisMonth = _context.Users.Count(u => u.AccountCreated >= startOfMonth),
                TotalProducts = _context.Products.Count(),
                TotalOrders = _context.Orders.Count(),
                Revenue = _context.Orders.Sum(o => o.TotalAmount),
                RevenueChange = CalculateRevenueChange(),
                RecentOrders = GetRecentOrders(),
            };
        }

        public SalesData GetSalesData(string period)
        {
            var now = DateTime.UtcNow;
            var data = new SalesData
            {
                Labels = new List<string>(),
                Data = new List<decimal>()
            };

            if (period == "weekly")
            {
                var startDate = now.AddDays(-6);
                for (var i = 0; i < 7; i++)
                {
                    var date = startDate.AddDays(i);
                    data.Labels.Add(date.ToString("ddd"));
                    data.Data.Add(_context.Orders
                        .Where(o => o.OrderDate.Date == date.Date)
                        .Sum(o => o.TotalAmount));
                }
            }
            else if (period == "monthly")
            {
                var daysInMonth = DateTime.DaysInMonth(now.Year, now.Month);
                for (var i = 1; i <= daysInMonth; i++)
                {
                    data.Labels.Add(i.ToString());
                    data.Data.Add(_context.Orders
                        .Where(o => o.OrderDate.Year == now.Year &&
                                   o.OrderDate.Month == now.Month &&
                                   o.OrderDate.Day == i)
                        .Sum(o => o.TotalAmount));
                }
            }
            else // yearly
            {
                for (var i = 1; i <= 12; i++)
                {
                    data.Labels.Add(new DateTime(now.Year, i, 1).ToString("MMM"));
                    data.Data.Add(_context.Orders
                        .Where(o => o.OrderDate.Year == now.Year && o.OrderDate.Month == i)
                        .Sum(o => o.TotalAmount));
                }
            }

            return data;
        }

        //public UserActivityData GetUserActivity()
        //{
        //    var now = DateTime.UtcNow;
        //    var startDate = now.AddDays(-29); // Last 30 days

        //    var result = new UserActivityData
        //    {
        //        Labels = new List<string>(),
        //        Logins = new List<int>(),
        //        Registrations = new List<int>()
        //    };

        //    for (var i = 0; i < 30; i++)
        //    {
        //        var date = startDate.AddDays(i);
        //        result.Labels.Add(date.ToString("MMM dd"));
        //        result.Logins.Add(_context.Orders
        //            .Count(a => a.ActivityType == "Login" && a.Timestamp.Date == date.Date));
        //        result.Registrations.Add(_context.Users
        //            .Count(u => u.AccountCreated.Date == date.Date));
        //    }

        //    return result;
        //}

        private decimal CalculateRevenueChange()
        {
            var now = DateTime.UtcNow;
            var currentMonthRevenue = _context.Orders
                .Where(o => o.OrderDate.Year == now.Year && o.OrderDate.Month == now.Month)
                .Sum(o => o.TotalAmount);

            var lastMonthRevenue = _context.Orders
                .Where(o => o.OrderDate.Year == now.AddMonths(-1).Year &&
                           o.OrderDate.Month == now.AddMonths(-1).Month)
                .Sum(o => o.TotalAmount);

            if (lastMonthRevenue == 0) return 100;
            return ((currentMonthRevenue - lastMonthRevenue) / lastMonthRevenue) * 100;
        }

        private List<RecentOrder> GetRecentOrders(int count = 5)
        {
            return _context.Orders
                .OrderByDescending(o => o.OrderDate)
                .Take(count)
                .Select(o => new RecentOrder
                {
                    OrderId = o.Id.ToString(),
                    CustomerName = o.User.FirstName,
                    Amount = o.TotalAmount,
                    Status = o.OrderStatus.ToString(),
                    Date = o.OrderDate.ToString("MMM dd, yyyy")
                })
                .ToList();
        }

        private List<ActiveUser> GetAllUsers()
        {
            return _context.Users
                .Select(u => new ActiveUser
                {
                    UserId = u.Id,
                    Name = u.FirstName,
                    Email = u.Email,
                    LastActivity = _context.Orders
                        .Where(a => a.UserId == u.Id)
                        .OrderByDescending(a => a.OrderDate)
                        .Select(a => a.OrderDate.ToString("MMM dd, HH:mm"))
                        .FirstOrDefault() ?? "No activity",
                    Role = _context.UserRoles.Any(ur => ur.UserId == u.Id && ur.RoleId == "Admin") ? "Admin" : "User"
                })
                .ToList();
        }

    }
}