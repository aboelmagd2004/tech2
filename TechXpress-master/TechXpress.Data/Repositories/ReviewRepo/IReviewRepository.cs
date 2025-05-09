using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using TechXpress.Data.Models;
using TechXpress.Data.Repositories.GenericRepository;

namespace TechXpress.Data.Repositories.ReviewRepo
{
    public interface IReviewRepository : IGenericRepository<Review>
    {
        // Basic test method
        string TestRepository();

        // Core review operations
        Task<IEnumerable<Review>> GetReviewsForProductAsync(int productId);
        Task<IEnumerable<Review>> GetReviewsByUserAsync(string userId);
        Task<Review> GetReviewWithDetailsAsync(int reviewId);

        // Rating operations
        Task<double> GetAverageRatingForProductAsync(int productId);
        Task<Dictionary<int, int>> GetRatingDistributionForProductAsync(int productId);

        // Advanced filtering
        Task<IEnumerable<Review>> GetFilteredReviewsAsync(
            Expression<Func<Review, bool>> filter = null,
            Func<IQueryable<Review>, IOrderedQueryable<Review>> orderBy = null,
            string includeProperties = "",
            int? skip = null,
            int? take = null);

        // Review management
        Task UpdateReviewAsync(int reviewId, int newRating, string newComment);
        Task<bool> HasUserReviewedProductAsync(string userId, int productId);
    }
}