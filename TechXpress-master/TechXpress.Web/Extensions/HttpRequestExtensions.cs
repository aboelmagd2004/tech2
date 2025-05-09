using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace TechXpress.Web.Extensions
{
    public static class HttpRequestExtensions
    {
        public static bool IsAjaxRequest(this HttpRequest request)
        {
            return request?.Headers["X-Requested-With"].ToString() == "XMLHttpRequest";
        }
    }
}
