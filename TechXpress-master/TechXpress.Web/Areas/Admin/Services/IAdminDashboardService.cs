using TechXpress.Web.Areas.Admin.Models;

namespace TechXpress.Web.Areas.Admin.Services
{
    public interface IAdminDashboardService
    {
        DashboardViewModel GetDashboardData();
        SalesData GetSalesData(string period);
    }
}
