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
            
            if (typeof(TDbContext).Name == "ShardingDbContext")
            {
                var constructor = typeof(TDbContext).GetConstructor(
                    new[] { typeof(DbContextOptions<TDbContext>), typeof(PersonContext) });
                if (constructor != null)
                {
                    return (TDbContext)constructor.Invoke(new object[] { optionsBuilder.Options, personContext });
                }
            }
            
            return (TDbContext)Activator.CreateInstance(typeof(TDbContext), optionsBuilder.Options)!;
        }
    }
}
