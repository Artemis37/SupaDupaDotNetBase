using Microsoft.EntityFrameworkCore;
using Shared.Application.Interfaces;
using Shared.Application.Context;

namespace Shared.Infrastructure.Data
{
    public class ShardingDbContextProvider<TDbContext> : IShardingDbContextProvider<TDbContext> 
        where TDbContext : DbContext
    {
        private readonly IDbContextFactory _dbContextFactory;
        private readonly IPersonContextProvider _personContextProvider;

        public ShardingDbContextProvider(
            IDbContextFactory dbContextFactory,
            IPersonContextProvider personContextProvider)
        {
            _dbContextFactory = dbContextFactory;
            _personContextProvider = personContextProvider;
        }

        public TDbContext GetDbContext()
        {
            var personContext = _personContextProvider.GetContext();
            
            if (personContext.ShardId == null)
            {
                throw new InvalidOperationException("ShardId is not set in PersonContext. Ensure PersonContextMiddleware is registered and personId header is provided.");
            }

            var connectionString = _dbContextFactory.CreateConnectionString(personContext.ShardId.Value);
            var optionsBuilder = new DbContextOptionsBuilder<TDbContext>();
            optionsBuilder.UseSqlServer(connectionString);
            
            // Check if TDbContext is ShardingDbContext and requires IPersonContextProvider
            if (typeof(TDbContext).Name == "ShardingDbContext")
            {
                // Use reflection to create with both parameters
                var constructor = typeof(TDbContext).GetConstructor(
                    new[] { typeof(DbContextOptions<TDbContext>), typeof(IPersonContextProvider) });
                if (constructor != null)
                {
                    return (TDbContext)constructor.Invoke(new object[] { optionsBuilder.Options, _personContextProvider });
                }
            }
            
            return (TDbContext)Activator.CreateInstance(typeof(TDbContext), optionsBuilder.Options)!;
        }
    }
}
