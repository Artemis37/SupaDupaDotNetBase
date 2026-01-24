using Microsoft.EntityFrameworkCore;

namespace Shared.Application.Interfaces
{
    public interface IUnitOfWork
    {
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
        TDbContext GetDbContext<TDbContext>() where TDbContext : DbContext;
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}
