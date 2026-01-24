using Microsoft.EntityFrameworkCore;

namespace Shared.Application.Interfaces
{
    public interface IShardingDbContextProvider<TDbContext> where TDbContext : DbContext
    {
        TDbContext GetDbContext();
    }
}
