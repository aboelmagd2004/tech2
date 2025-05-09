using System.Collections.Generic;
using System.Threading.Tasks;
using TechXpress.Data.Models;
using TechXpress.Web.Services.Implementations;

namespace TechXpress.Web.Services.Interfaces
{
    public interface IAICommerceService
    {
        Task<string> GenerateProductDescriptions(ProductViewModel product);
        Task<string> AnalyzeCustomerSentiment(List<Review> reviews);
        Task<string> GeneratePersonalizedPromotions(ApplicationUser customer, List<Order> orderHistory);
        Task<string> OptimizePricingStrategy(List<ProductViewModel> products, MarketData marketData);
        Task<string> PredictInventoryNeeds(List<ProductViewModel> products, SalesForecast forecast);
    }
}
