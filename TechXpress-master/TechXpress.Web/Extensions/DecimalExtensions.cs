namespace TechXpress.Web.Extensions
{
    public static class DecimalExtensions
    {
        public static string ToCurrencyString(this decimal amount)
        {
            return amount.ToString("C2"); // "$1.00" format depending on culture
        }
       
    }
}
