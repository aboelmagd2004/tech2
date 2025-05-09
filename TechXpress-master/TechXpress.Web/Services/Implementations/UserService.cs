using Microsoft.EntityFrameworkCore;
using TechXpress.Data.Models.Contexts;
using TechXpress.Web.Services.Interfaces;

namespace TechXpress.Web.Services.Implementations
{
    public class UserService : IUserService
    {
        private readonly TechXpressDbContext _context;

        public UserService(TechXpressDbContext context)
        {
            _context = context;
        }

        public async Task<string> GetProfilePicturePath(string userId)
        {
            var user = await _context.Users.FindAsync(userId);
            return user?.ProfilePictureUrl;
        }


        public async Task UpdateProfilePicture(string userId, string profilePicturePath)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user != null)
            {
                user.ProfilePictureUrl = profilePicturePath;
                _context.Users.Update(user);
                await _context.SaveChangesAsync();
            }
        }
    }

}
