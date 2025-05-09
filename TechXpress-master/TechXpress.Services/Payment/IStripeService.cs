using System.Threading.Tasks;
using Stripe;

namespace TechXpress.Services.Payment
{
    public interface IStripeService
    {
        // ينشئ PaymentIntent ويربطه بالأوردر عبر Metadata
        Task<PaymentIntent> CreatePaymentIntentAsync(long amount, string currency, int orderId);

        // يجلب حالة الـ PaymentIntent من Stripe
        Task<PaymentIntent> RetrievePaymentIntentAsync(string paymentIntentId);
    }
}
