using TechXpress.Data.Models;

namespace TechXpress.Web.Services.Interfaces
{
    public interface IAIAssistantService
    {
        Task<string> GetProductRecommendations(List<ProductViewModel> viewedProducts);
        Task<string> GetOrderPrioritization(List<Order> orders);
        Task<string> GetCustomerQueryResponse(string customerQuery);
    }
}
