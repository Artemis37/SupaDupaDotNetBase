using Microsoft.EntityFrameworkCore;
using Shared.Application.Interfaces;
using Shared.Application.Context;

namespace Shared.Infrastructure.Data
{
    public class ShardingDbContextProvider<TDbContext> : IShardingDbContextProvider<TDbContext> 
        where TDbContext : DbContext
    {
        private readonly IDbContextFactory _dbContextFactory;

        public ShardingDbContextProvider(IDbContextFactory dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        public TDbContext GetDbContext()
        {
            var personContext = PersonContextProvider.Current;
            if (personContext?.ShardId == null)
            {
                throw new InvalidOperationException("ShardId is not set in PersonContext. Ensure PersonContextMiddleware is registered and personId header is provided.");
            }

            var connectionString = _dbContextFactory.CreateConnectionString(personContext.ShardId.Value);
            var optionsBuilder = new DbContextOptionsBuilder<TDbContext>();
            optionsBuilder.UseSqlServer(connectionString, sqlServerOptions => 
                sqlServerOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorNumbersToAdd: null));
            
            return (TDbContext)Activator.CreateInstance(typeof(TDbContext), optionsBuilder.Options)!;
        }
    }
}
