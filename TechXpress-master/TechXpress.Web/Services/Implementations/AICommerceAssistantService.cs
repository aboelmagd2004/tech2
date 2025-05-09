using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using TechXpress.Data.Models;
using TechXpress.Web.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Stripe;
using Review = TechXpress.Data.Models.Review;

namespace TechXpress.Web.Services.Implementations
{
    public class AICommerceAssistantService : IAIAssistantService, IAICommerceService
    {
        private readonly HttpClient _client;
        private readonly string _apiKey;
        private readonly string _geminiApiUrl = "https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash-latest:generateContent";
        private static DateTime _lastRequestTime;
        private static readonly TimeSpan MinRequestInterval = TimeSpan.FromSeconds(1);

        public AICommerceAssistantService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _client = httpClientFactory.CreateClient();
            _apiKey = configuration.GetValue<string>("AI:ApiKey");
        }

        #region Core AI Assistant Functions
        public async Task<string> GetProductRecommendations(List<ProductViewModel> viewedProducts)
        {
            var prompt = new StringBuilder("Based on these viewed products, recommend similar or complementary items:\n");

            foreach (var product in viewedProducts)
            {
                prompt.AppendLine($"- {product.Name} ({product.Category}, ${product.Price}, Rating: {product.Reviews.Average(r => r.Rating)})");
            }

            prompt.AppendLine("\nProvide recommendations in this format:");
            prompt.AppendLine("1. [Product Name] - [Brief Reason] (Category: X, Price: $Y)");

            return await GetAIResponse(prompt.ToString());
        }

        public async Task<string> GetOrderPrioritization(List<Order> orders)
        {
            var prompt = new StringBuilder("Analyze these orders and prioritize them for fulfillment:\n\n");

            foreach (var order in orders)
            {
                prompt.AppendLine($"Order #{order.Id}:");
                prompt.AppendLine($"- Items: {order.ItemCount}");
                prompt.AppendLine($"- Total: ${order.TotalAmount}");
                prompt.AppendLine($"- Order Date: {order.OrderDate}");
                prompt.AppendLine();
            }

            prompt.AppendLine("Consider these factors:");
            prompt.AppendLine("- VIP customers get priority");
            prompt.AppendLine("- Express shipping orders should be processed first");
            prompt.AppendLine("- Older orders should be prioritized");
            prompt.AppendLine("- Larger orders may need more processing time");

            prompt.AppendLine("\nProvide the prioritized order numbers in sequence with brief explanations.");

            return await GetAIResponse(prompt.ToString());
        }

        public async Task<string> GetCustomerQueryResponse(string customerQuery)
        {
            var prompt = new StringBuilder("You are an e-commerce customer service assistant. ");
            prompt.AppendLine("Respond to this customer query professionally and helpfully:\n\n");
            prompt.AppendLine(customerQuery);
            prompt.AppendLine("\nIf the query is about:");
            prompt.AppendLine("- Order status: Ask for order number first");
            prompt.AppendLine("- Returns: Provide return policy summary");
            prompt.AppendLine("- Products: Offer specific details or alternatives");
            prompt.AppendLine("- Complaints: Apologize and escalate if needed");

            return await GetAIResponse(prompt.ToString());
        }
        #endregion

        #region Expanded AI Commerce Services
        public async Task<string> GenerateProductDescriptions(ProductViewModel product)
        {
            var prompt = $"Write a compelling 150-word product description for:\n\n";
            prompt += $"Name: {product.Name}\n";
            prompt += $"Category: {product.Category}\n";
            prompt += "Key Features:\n";
            

                foreach (var specification in product.Specifications ?? Enumerable.Empty<ProductSpecification>()) { 
                prompt += $"{specification.Key}: {specification.Value}\n";
            }
            prompt += "Include SEO-friendly keywords and highlight unique selling points.";

            return await GetAIResponse(prompt);
        }

        public async Task<string> AnalyzeCustomerSentiment(List<Review> reviews)
        {
            var prompt = new StringBuilder("Analyze these product reviews for overall sentiment and key themes:\n\n");

            foreach (var review in reviews.Take(10)) // Limit to 10 for token constraints
            {
                prompt.AppendLine($"Rating: {review.Rating}/5");
                prompt.AppendLine($"Review: {review.Comment}");
                prompt.AppendLine();
            }

            prompt.AppendLine("Provide:");
            prompt.AppendLine("- Overall sentiment score (1-10)");
            prompt.AppendLine("- 3 key positive themes");
            prompt.AppendLine("- 3 key improvement areas");
            prompt.AppendLine("- Suggested actions for the product team");

            return await GetAIResponse(prompt.ToString());
        }

        public async Task<string> GeneratePersonalizedPromotions(ApplicationUser customer, List<Order> orderHistory)
        {
            var prompt = new StringBuilder($"Create personalized promotion ideas for customer:\n\n");
            prompt.AppendLine($"Name: {customer.FirstName} {customer.LastName}");
            prompt.AppendLine($"Total Spend: ${orderHistory.Sum(o => o.TotalAmount)}");
            prompt.AppendLine($"Favorite Categories: {GetTopCategories(orderHistory)}");
            prompt.AppendLine("\nSuggest:");
            prompt.AppendLine("- 3 discount offers tailored to their preferences");
            prompt.AppendLine("- 1 loyalty reward idea");
            prompt.AppendLine("- 1 cross-sell opportunity");
            prompt.AppendLine("- Email subject line and brief body text");

            return await GetAIResponse(prompt.ToString());
        }

        public async Task<string> OptimizePricingStrategy(List<ProductViewModel> products, MarketData marketData)
        {
            var prompt = new StringBuilder("Suggest pricing optimizations based on this market data:\n\n");
            prompt.AppendLine($"Competitor Average Prices: {marketData.CompetitorPrices}");
            prompt.AppendLine($"Demand Trends: {marketData.DemandTrends}");
            prompt.AppendLine($"Inventory Levels: {marketData.InventoryLevels}");
            prompt.AppendLine("\nFor these products:\n");

            foreach (var product in products.Take(5)) // Limit examples
            {
                prompt.AppendLine($"{product.Name} (Current: ${product.Price})");
            }

            prompt.AppendLine("\nRecommend:");
            prompt.AppendLine("- Which products to increase/decrease price");
            prompt.AppendLine("- Suggested new prices with rationale");
            prompt.AppendLine("- Any bundle or promotion opportunities");

            return await GetAIResponse(prompt.ToString());
        }

        public async Task<string> PredictInventoryNeeds(List<ProductViewModel> products, SalesForecast forecast)
        {
            var prompt = new StringBuilder("Predict inventory needs based on:\n\n");
            prompt.AppendLine($"Season: {forecast.Season}");
            prompt.AppendLine($"Upcoming Holidays: {string.Join(", ", forecast.UpcomingHolidays)}");
            prompt.AppendLine($"Historical Sales: {forecast.HistoricalSalesData}");
            prompt.AppendLine("\nFor these products:\n");

            foreach (var product in products.Take(5))
            {
                prompt.AppendLine($"{product.Name} (Current Stock: {product.StockQuantity}");
            }

            prompt.AppendLine("\nRecommend:");
            prompt.AppendLine("- Quantity to order for each product");
            prompt.AppendLine("- Timing for reorders");
            prompt.AppendLine("- Any products to discontinue");

            return await GetAIResponse(prompt.ToString());
        }
        #endregion

        #region Helper Methods
        private async Task<string> GetAIResponse(string prompt)
        {
            // Rate limiting
            var timeSinceLastRequest = DateTime.UtcNow - _lastRequestTime;
            if (timeSinceLastRequest < MinRequestInterval)
            {
                await Task.Delay(MinRequestInterval - timeSinceLastRequest);
            }

            try
            {
                var requestBody = new
                {
                    contents = new[] {
                        new {
                            parts = new[] {
                                new {
                                    text = prompt
                                }
                            }
                        }
                    },
                    generationConfig = new
                    {
                        temperature = 0.7,
                        topP = 0.9,
                        maxOutputTokens = 2048
                    }
                };

                var jsonRequest = JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

                var response = await _client.PostAsync($"{_geminiApiUrl}?key={_apiKey}", content);
                _lastRequestTime = DateTime.UtcNow;

                if (!response.IsSuccessStatusCode)
                {
                    var errorBody = await response.Content.ReadAsStringAsync();
                    return $"AI service error: {errorBody}";
                }

                var responseStr = await response.Content.ReadAsStringAsync();
                dynamic responseObj = JsonConvert.DeserializeObject(responseStr);
                return responseObj?.candidates?[0]?.content?.parts?[0]?.text ?? "No response generated";
            }
            catch (Exception ex)
            {
                return $"Error accessing AI service: {ex.Message}";
            }
        }

        private string GetTopCategories(List<Order> orders)
        {
            var categories = orders
                .SelectMany(o => o.OrderDetails)
                .GroupBy(i => i.Product.Category)
                .OrderByDescending(g => g.Count())
                .Take(3)
                .Select(g => g.Key);

            return string.Join(", ", categories);
        }
        #endregion
    }

 

    public class MarketData
    {
        public string CompetitorPrices { get; set; }
        public string DemandTrends { get; set; }
        public string InventoryLevels { get; set; }
    }

    public class SalesForecast
    {
        public string Season { get; set; }
        public List<string> UpcomingHolidays { get; set; }
        public string HistoricalSalesData { get; set; }
    }
}
