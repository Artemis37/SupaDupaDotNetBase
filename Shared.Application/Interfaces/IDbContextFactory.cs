namespace Shared.Application.Interfaces
{
    public interface IDbContextFactory
    {
        string CreateConnectionString(int shardId);
    }
}
