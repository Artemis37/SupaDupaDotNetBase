namespace Vehicle.Infrastructure.Data
{
    public interface IShardingDbContextProvider
    {
        ShardingDbContext GetDbContext();
    }
}
