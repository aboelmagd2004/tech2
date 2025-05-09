using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace TechXpress.Services.GenericServices
{
    public interface IGenericService<T> where T : class
    {
        Task<IEnumerable<T>> GetAllAsync();
        Task<T> GetByIdAsync(object id);
        Task AddAsync(T entity);
        Task UpdateAsync(T entity);  // Changed to async
        Task DeleteAsync(T entity);  // Changed to async
        Task DeleteAsync(object id); // Added overload
        Task SaveAsync();

        // Optional advanced methods
        Task<bool> ExistsAsync(object id);
        Task<IEnumerable<T>> GetFilteredAsync(
            Expression<Func<T, bool>> filter = null,
            Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
            string includeProperties = "",
            int? skip = null,
            int? take = null);
    }
}