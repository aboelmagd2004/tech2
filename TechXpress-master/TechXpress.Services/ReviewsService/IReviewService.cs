using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using TechXpress.Data.Models;
using TechXpress.Services.GenericServices;

namespace TechXpress.Services.ReviewsService
{
    public interface IReviewService : IGenericService<Review>
    {
        Task<string> Test();

        // Review-specific operations
        Task<IEnumerable<Review>> GetReviewsForProductAsync(int productId);
        Task<IEnumerable<Review>> GetReviewsByUserAsync(string userId);
        Task<Review> GetReviewWithDetailsAsync(int reviewId);

        // Rating operations
        Task<double> GetAverageRatingForProductAsync(int productId);
        Task<Dictionary<int, int>> GetRatingDistributionForProductAsync(int productId);

        // Review management
        Task UpdateReviewAsync(int reviewId, int newRating, string newComment);
        Task<bool> HasUserReviewedProductAsync(string userId, int productId);

        // Advanced filtering
        Task<IEnumerable<Review>> GetFilteredReviewsAsync(
            Expression<Func<Review, bool>> filter = null,
            Func<IQueryable<Review>, IOrderedQueryable<Review>> orderBy = null,
            string includeProperties = "",
            int? skip = null,
            int? take = null);
    }
}