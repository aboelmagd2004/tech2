using System.Collections.Generic;

namespace TechXpress.Web.Areas.Admin.Models
{
    public class DashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int NewUsersThisMonth { get; set; }
        public int TotalProducts { get; set; }
        public int TotalOrders { get; set; }
        public decimal Revenue { get; set; }
        public decimal RevenueChange { get; set; }
        public List<RecentOrder> RecentOrders { get; set; }
        public List<ActiveUser> ActiveUsers { get; set; }
    }

    public class RecentOrder
    {
        public string OrderId { get; set; }
        public string CustomerName { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; }
        public string Date { get; set; }
    }

    public class ActiveUser
    {
        public string UserId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string LastActivity { get; set; }
        public string Role { get; set; }
    }

    public class SalesData
    {
        public List<string> Labels { get; set; }
        public List<decimal> Data { get; set; }
    }

    public class UserActivityData
    {
        public List<string> Labels { get; set; }
        public List<int> Logins { get; set; }
        public List<int> Registrations { get; set; }
    }
}