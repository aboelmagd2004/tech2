namespace TechXpress.Web.Models
{
    public class CreateIntentRequest
    {
        public long Amount { get; set; }
        public string Currency { get; set; }
        public int OrderId { get; set; }
    }
}
