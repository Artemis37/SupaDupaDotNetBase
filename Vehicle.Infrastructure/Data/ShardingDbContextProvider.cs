using Microsoft.EntityFrameworkCore;
using Vehicle.Infrastructure.Context;

namespace Vehicle.Infrastructure.Data
{
    public class ShardingDbContextProvider : IShardingDbContextProvider
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

        public ShardingDbContext GetDbContext()
        {
            var personContext = _personContextProvider.GetContext();
            
            if (personContext.ShardId == null)
            {
                throw new InvalidOperationException("ShardId is not set in PersonContext. Ensure PersonContextMiddleware is registered and personId header is provided.");
            }

            var connectionString = _dbContextFactory.CreateConnectionString(personContext.ShardId.Value);
            var optionsBuilder = new DbContextOptionsBuilder<ShardingDbContext>();
            optionsBuilder.UseSqlServer(connectionString);
            
            return new ShardingDbContext(optionsBuilder.Options);
        }
    }
}
