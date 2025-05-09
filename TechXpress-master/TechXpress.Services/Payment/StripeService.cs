using Microsoft.Extensions.Options;
using Stripe;

namespace TechXpress.Services.Payment
{
    public class StripeService : IStripeService
    {
        private readonly StripeSettings _settings;
        public StripeService(IOptions<StripeSettings> opts)
        {
            _settings = opts.Value;
            StripeConfiguration.ApiKey = _settings.SecretKey;
        }

        public async Task<PaymentIntent> CreatePaymentIntentAsync(long amount, string currency, int orderId)
        {
            var options = new PaymentIntentCreateOptions
            {
                Amount = amount,
                Currency = currency,
                AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
                {
                    Enabled = true
                },
                Metadata = new Dictionary<string, string>
                {
                    { "order_id", orderId.ToString() }
                }
            };
            var svc = new PaymentIntentService();
            return await svc.CreateAsync(options);
        }

        public async Task<PaymentIntent> RetrievePaymentIntentAsync(string paymentIntentId)
        {
            var svc = new PaymentIntentService();
            return await svc.GetAsync(paymentIntentId);
        }
    }
}
