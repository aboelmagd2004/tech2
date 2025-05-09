using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using TechXpress.Data.Models;
using TechXpress.Web.Services.Interfaces;
using Microsoft.Extensions.Configuration;

namespace TechXpress.Web.Services.Implementations
{
    public class AIAssistantService : IAIAssistantService
    {
        private readonly HttpClient _client;
        private readonly string _apiKey;
        private readonly string _geminiApiUrl = "https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash-latest:generateContent";
        private static DateTime _lastRequestTime;
        private static readonly TimeSpan MinRequestInterval = TimeSpan.FromSeconds(1);

        // Constructor to inject HttpClient and IConfiguration
        public AIAssistantService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _client = httpClientFactory.CreateClient();
            _apiKey = configuration.GetValue<string>("AI:ApiKey");
        }

        // 1. **Product Recommendations**
        public async Task<string> GetProductRecommendations(List<ProductViewModel> viewedProducts)
        {
            var prompt = "Based on the following products, recommend similar products:\n\n";
            foreach (var product in viewedProducts)
            {
                prompt += $"- {product.Name} (Category: {product.Category}, Price: {product.Price})\n";
            }
            prompt += "\nPlease provide a list of recommended products, including their category and price.";

            return await GetAIResponse(prompt);
        }

        // 2. **Order Prioritization**
        public async Task<string> GetOrderPrioritization(List<Order> orders)
        {
            var prompt = "Prioritize the following orders based on delivery time and customer importance:\n\n";
            prompt += "\nPlease provide a prioritized order list.";

            return await GetAIResponse(prompt);
        }

        // 3. **Customer Query Response**
        public async Task<string> GetCustomerQueryResponse(string customerQuery)
        {
            var prompt = $"Respond to the following customer query:\n\n{customerQuery}\nProvide a clear and helpful answer.";

            return await GetAIResponse(prompt);
        }

        private async Task<string> GetAIResponse(string prompt)
        {
            // Ensure that we don't make requests too quickly
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
                    }
                };

                var jsonRequest = JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

                var requestUri = $"{_geminiApiUrl}?key={_apiKey}";

                var response = await _client.PostAsync(requestUri, content);
                _lastRequestTime = DateTime.UtcNow;

                if (!response.IsSuccessStatusCode)
                {
                    var errorBody = await response.Content.ReadAsStringAsync();
                    if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                    {
                        return "The AI service is currently busy. Please try again in a minute.";
                    }
                    throw new Exception($"API error: {errorBody}");
                }

                var responseStr = await response.Content.ReadAsStringAsync();
                var responseMap = JsonConvert.DeserializeObject<Dictionary<string, object>>(responseStr);

                if (responseMap.ContainsKey("candidates"))
                {
                    var candidates = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(responseMap["candidates"].ToString());
                    if (candidates.Count > 0)
                    {
                        var candidate = candidates[0];
                        var contentResponse = JsonConvert.DeserializeObject<Dictionary<string, object>>(candidate["content"].ToString());
                        var parts = JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(contentResponse["parts"].ToString());
                        if (parts.Count > 0)
                        {
                            return parts[0]["text"];
                        }
                    }
                }

                return "No valid response from AI.";
            }
            catch (Exception e)
            {
                if (e.Message.Contains("rate limit"))
                {
                    return "The AI service is currently at capacity. Please try again in a few minutes.";
                }
                throw new Exception($"Error getting AI response: {e.Message}");
            }
        }
    }
}
