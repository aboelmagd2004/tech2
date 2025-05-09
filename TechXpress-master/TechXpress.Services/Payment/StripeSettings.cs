namespace TechXpress.Services.Payment
{
    /// <summary>
    /// يُحمَّل من appsettings.json
    /// </summary>
    public class StripeSettings
    {
        public string SecretKey { get; set; }
        public string PublishableKey { get; set; }
    }
}
