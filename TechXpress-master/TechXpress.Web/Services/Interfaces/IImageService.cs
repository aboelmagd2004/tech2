namespace TechXpress.Web.Services.Interfaces
{
    public interface IImageService
    {
        Task<string> UploadImageAsync(IFormFile imageFile);
        Task<bool> DeleteImageAsync(string imageUrl);
    }

}
