using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace TechXpress.Web.Areas.Admin.Services
{
    public interface IImageService
    {
        Task<string> GetPathAsync(IFormFile imageFile);
    }
}
