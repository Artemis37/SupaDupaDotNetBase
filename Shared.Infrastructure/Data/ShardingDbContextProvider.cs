using Microsoft.EntityFrameworkCore;
using Shared.Application.Interfaces;
using Shared.Application.Context;

namespace Shared.Infrastructure.Data
{
    public class ShardingDbContextProvider<TDbContext> : IShardingDbContextProvider<TDbContext> 
        where TDbContext : DbContext
    {
        private readonly IDbContextFactory _dbContextFactory;
        private readonly PersonContext _personContext;

        public ShardingDbContextProvider(
            IDbContextFactory dbContextFactory,
            PersonContext personContext)
        {
            _dbContextFactory = dbContextFactory;
            _personContext = personContext;
        }

        public TDbContext GetDbContext()
        {
            if (_personContext.ShardId == null)
            {
                throw new InvalidOperationException("ShardId is not set in PersonContext. Ensure PersonContextMiddleware is registered and personId header is provided.");
            }

            var connectionString = _dbContextFactory.CreateConnectionString(_personContext.ShardId.Value);
            var optionsBuilder = new DbContextOptionsBuilder<TDbContext>();
            optionsBuilder.UseSqlServer(connectionString, sqlServerOptions => 
                sqlServerOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorNumbersToAdd: null));
            
            if (typeof(TDbContext).Name == "ShardingDbContext")
            {
                var constructor = typeof(TDbContext).GetConstructor(
                    new[] { typeof(DbContextOptions<TDbContext>), typeof(PersonContext) });
                if (constructor != null)
                {
                    return (TDbContext)constructor.Invoke(new object[] { optionsBuilder.Options, _personContext });
                }
            }
            
            return (TDbContext)Activator.CreateInstance(typeof(TDbContext), optionsBuilder.Options)!;
        }
    }
}
