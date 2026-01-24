using Microsoft.EntityFrameworkCore;
using Vehicle.Infrastructure.Data;

namespace Vehicle.Infrastructure.Repositories
{
    public class BaseRepository<T> where T : class
    {
        protected readonly ShardingDbContext _context;

        public BaseRepository(ShardingDbContext context)
        {
            _context = context;
        }

        public virtual async Task<T?> GetByIdAsync(int id)
        {
            return await _context.Set<T>().FindAsync(id);
        }

        public virtual async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _context.Set<T>().ToListAsync();
        }

        public virtual async Task<T> AddAsync(T entity)
        {
            await _context.Set<T>().AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public virtual async Task UpdateAsync(T entity)
        {
            _context.Set<T>().Update(entity);
            await _context.SaveChangesAsync();
        }

        public virtual async Task DeleteAsync(T entity)
        {
            _context.Set<T>().Remove(entity);
            await _context.SaveChangesAsync();
        }

        public virtual async Task<bool> ExistsAsync(int id)
        {
            var entity = await _context.Set<T>().FindAsync(id);
            return entity != null;
        }
    }
}
