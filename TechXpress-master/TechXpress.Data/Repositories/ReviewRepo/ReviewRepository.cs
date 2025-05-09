using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TechXpress.Data.Models;
using TechXpress.Data.Models.Contexts;
using TechXpress.Data.Repositories.GenericRepository;

namespace TechXpress.Data.Repositories.ReviewRepo
{
    public class ReviewRepository : GenericRepository<Review>, IReviewRepository
    {
        public ReviewRepository(TechXpressDbContext context) : base(context)
        {
        }

        public string TestRepository()
        {
            return "Review Repository is operational";
        }

        public async Task<IEnumerable<Review>> GetReviewsForProductAsync(int productId)
        {
            return await _context.Set<Review>()
                .Where(r => r.ProductId == productId)
                .Include(r => r.User)
                .OrderByDescending(r => r.CreatedAt)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IEnumerable<Review>> GetReviewsByUserAsync(string userId)
        {
            return await _context.Set<Review>()
                .Where(r => r.UserId == userId)
                .Include(r => r.Product)
                .OrderByDescending(r => r.CreatedAt)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Review> GetReviewWithDetailsAsync(int reviewId)
        {
            return await _context.Set<Review>()
                .Include(r => r.Product)
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Id == reviewId);
        }

        public async Task<double> GetAverageRatingForProductAsync(int productId)
        {
            return await _context.Set<Review>()
                .Where(r => r.ProductId == productId)
                .AverageAsync(r => (double)r.Rating);
        }

        public async Task<Dictionary<int, int>> GetRatingDistributionForProductAsync(int productId)
        {
            return await _context.Set<Review>()
                .Where(r => r.ProductId == productId)
                .GroupBy(r => r.Rating)
                .Select(g => new { Rating = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Rating, x => x.Count);
        }

        public async Task<IEnumerable<Review>> GetFilteredReviewsAsync(
            Expression<Func<Review, bool>> filter = null,
            Func<IQueryable<Review>, IOrderedQueryable<Review>> orderBy = null,
            string includeProperties = "",
            int? skip = null,
            int? take = null)
        {
            IQueryable<Review> query = _context.Set<Review>();

            if (filter != null)
            {
                query = query.Where(filter);
            }

            foreach (var includeProperty in includeProperties.Split
                (new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                query = query.Include(includeProperty);
            }

            if (orderBy != null)
            {
                query = orderBy(query);
            }

            if (skip.HasValue)
            {
                query = query.Skip(skip.Value);
            }

            if (take.HasValue)
            {
                query = query.Take(take.Value);
            }

            return await query.AsNoTracking().ToListAsync();
        }

        public async Task UpdateReviewAsync(int reviewId, int newRating, string newComment)
        {
            var review = await _context.Set<Review>().FindAsync(reviewId);
            if (review != null)
            {
                review.Rating = newRating;
                review.Comment = newComment;
                review.UpdatedAt = DateTime.UtcNow;
                _context.Set<Review>().Update(review);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> HasUserReviewedProductAsync(string userId, int productId)
        {
            return await _context.Set<Review>()
                .AnyAsync(r => r.UserId == userId && r.ProductId == productId);
        }
    }
}