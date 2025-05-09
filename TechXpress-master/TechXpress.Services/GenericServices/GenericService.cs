using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using TechXpress.Data.Models;
using TechXpress.Data.Repositories.GenericRepository;

namespace TechXpress.Services.GenericServices
{
    public class GenericService<T> : IGenericService<T> where T : class
    {
        private readonly IGenericRepository<T> _repository;

        public GenericService(IGenericRepository<T> repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _repository.GetAllAsync();
        }

        public async Task<T> GetByIdAsync(object id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task AddAsync(T entity)
        {
            await _repository.AddAsync(entity);

        }

        public void Update(T entity)
        {
            _repository.Update(entity);
        }

        public async Task UpdateAsync(T entity)
        {
            _repository.Update(entity);
            await _repository.SaveAsync();
        }

        public void Delete(T entity)
        {
            _repository.Delete(entity);
        }

        public async Task DeleteAsync(T entity)
        {
            _repository.Delete(entity);
            await _repository.SaveAsync();
        }

        public async Task DeleteAsync(object id)
        {
            var entity = await GetByIdAsync(id);
            if (entity != null)
            {
                await DeleteAsync(entity);
            }
        }

        public async Task SaveAsync()
        {
            await _repository.SaveAsync();
        }

        public async Task<bool> ExistsAsync(object id)
        {
            return await _repository.GetByIdAsync(id) != null;
        }

        public async Task<IEnumerable<T>> GetFilteredAsync(
            Expression<Func<T, bool>> filter = null,
            Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
            string includeProperties = "",
            int? skip = null,
            int? take = null)
        {
            return await _repository.GetFilteredAsync(
                filter,
                orderBy,
                includeProperties,
                skip,
                take);
        }
    }
}