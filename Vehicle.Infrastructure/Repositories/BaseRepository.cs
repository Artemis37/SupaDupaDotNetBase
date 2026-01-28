using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shared.Application.Interfaces;
using Vehicle.Infrastructure.Data;

namespace Vehicle.Infrastructure.Repositories
{
    public class BaseRepository<T> where T : class
    {
        protected ShardingDbContext Context;
        private readonly IServiceProvider _serviceProvider;

        public BaseRepository(ShardingDbContext context, IServiceProvider serviceProvider)
        {
            Context = context;
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Reloads the ShardingDbContext from DI after PersonContext has been set
        /// </summary>
        public void ReloadShardingDbContext()
        {
            var provider = _serviceProvider.GetRequiredService<IShardingDbContextProvider<ShardingDbContext>>();
            Context = provider.GetDbContext();
        }

        public virtual async Task<T?> GetByIdAsync(int id)
        {
            return await Context.Set<T>().FindAsync(id);
        }

        public virtual async Task<IEnumerable<T>> GetAllAsync()
        {
            return await Context.Set<T>().ToListAsync();
        }

        public virtual IQueryable<T> GetQueryable()
        {
            return Context.Set<T>();
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

        public virtual async Task<int> SaveChangesAsync()
        {
            return await Context.SaveChangesAsync();
        }
    }
}
