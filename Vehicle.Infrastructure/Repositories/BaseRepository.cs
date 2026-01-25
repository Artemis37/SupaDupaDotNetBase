using Microsoft.EntityFrameworkCore;
using Shared.Application.Interfaces;
using Vehicle.Infrastructure.Data;

namespace Vehicle.Infrastructure.Repositories
{
    public class BaseRepository<T> where T : class
    {
        protected readonly IUnitOfWork _unitOfWork;
        protected ShardingDbContext Context => _unitOfWork.GetDbContext<ShardingDbContext>();

        public BaseRepository(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public virtual async Task<T?> GetByIdAsync(int id)
        {
            return await Context.Set<T>().FindAsync(id);
        }

        public virtual async Task<IEnumerable<T>> GetAllAsync()
        {
            return await Context.Set<T>().ToListAsync();
        }

        public virtual async Task<T> AddAsync(T entity)
        {
            await Context.Set<T>().AddAsync(entity);
            return entity;
        }

        public virtual async Task UpdateAsync(T entity)
        {
            Context.Set<T>().Update(entity);
        }

        public virtual async Task DeleteAsync(T entity)
        {
            Context.Set<T>().Remove(entity);
        }

        public virtual async Task<bool> ExistsAsync(int id)
        {
            var entity = await Context.Set<T>().FindAsync(id);
            return entity != null;
        }
    }
}
