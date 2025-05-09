using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using TechXpress.Data.Models;
using TechXpress.Data.Repositories.ReviewRepo;
using TechXpress.Data.UnitOfWork;
using TechXpress.Services.GenericServices;

namespace TechXpress.Services.ReviewsService
{
    public class ReviewService : GenericService<Review>, IReviewService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IReviewRepository _reviewRepository;

        public ReviewService(IUnitOfWork unitOfWork)
            : base(unitOfWork.Reviews)
        {
            _unitOfWork = unitOfWork;
            _reviewRepository = unitOfWork.Reviews as IReviewRepository;
        }

        public async Task<string> Test()
        {
            var reviews = await _reviewRepository.GetFilteredReviewsAsync(take: 5);
            return $"{reviews.Count()} sample reviews loaded | Review Service is operational";
        }

        public async Task<IEnumerable<Review>> GetReviewsForProductAsync(int productId)
        {
            return await _reviewRepository.GetReviewsForProductAsync(productId);
        }

        public async Task<IEnumerable<Review>> GetReviewsByUserAsync(string userId)
        {
            return await _reviewRepository.GetReviewsByUserAsync(userId);
        }

        public async Task<Review> GetReviewWithDetailsAsync(int reviewId)
        {
            return await _reviewRepository.GetReviewWithDetailsAsync(reviewId);
        }

        public async Task<double> GetAverageRatingForProductAsync(int productId)
        {
            return await _reviewRepository.GetAverageRatingForProductAsync(productId);
        }

        public async Task<Dictionary<int, int>> GetRatingDistributionForProductAsync(int productId)
        {
            return await _reviewRepository.GetRatingDistributionForProductAsync(productId);
        }

        public async Task UpdateReviewAsync(int reviewId, int newRating, string newComment)
        {
            await _reviewRepository.UpdateReviewAsync(reviewId, newRating, newComment);
            await _unitOfWork.CompleteAsync();
        }

        public async Task<bool> HasUserReviewedProductAsync(string userId, int productId)
        {
            return await _reviewRepository.HasUserReviewedProductAsync(userId, productId);
        }

        public async Task<IEnumerable<Review>> GetFilteredReviewsAsync(
            Expression<Func<Review, bool>> filter = null,
            Func<IQueryable<Review>, IOrderedQueryable<Review>> orderBy = null,
            string includeProperties = "",
            int? skip = null,
            int? take = null)
        {
            return await _reviewRepository.GetFilteredReviewsAsync(
                filter,
                orderBy,
                includeProperties,
                skip,
                take);
        }
    }
}