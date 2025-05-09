namespace TechXpress.Web.Extensions
{
    public static class IntegerExtensions
    {
        public static string ToCurrencyString(this int amount)
        {
            return amount.ToString("C2"); // "$1.00" format depending on culture
        }
    }
}
