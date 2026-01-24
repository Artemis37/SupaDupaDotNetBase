namespace Vehicle.Infrastructure.Data
{
    public interface IDbContextFactory
    {
        string CreateConnectionString(int shardId);
    }
}
