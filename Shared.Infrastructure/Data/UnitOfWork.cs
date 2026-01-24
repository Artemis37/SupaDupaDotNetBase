using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Shared.Application.Interfaces;
using Shared.Infrastructure.Context;

namespace Shared.Infrastructure.Data
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly Dictionary<Type, DbContext> _dbContexts = new();
        private readonly IPersonContextProvider _personContextProvider;
        private readonly IServiceProvider _serviceProvider;
        private IDbContextTransaction? _currentTransaction;

        public UnitOfWork(IPersonContextProvider personContextProvider, IServiceProvider serviceProvider)
        {
            _personContextProvider = personContextProvider;
            _serviceProvider = serviceProvider;
        }

        public TDbContext GetDbContext<TDbContext>() where TDbContext : DbContext
        {
            var dbContextType = typeof(TDbContext);
            
            if (!_dbContexts.TryGetValue(dbContextType, out var dbContext))
            {
                // Resolve the provider for this DbContext type
                var providerType = typeof(IShardingDbContextProvider<>).MakeGenericType(dbContextType);
                var provider = _serviceProvider.GetRequiredService(providerType);
                var getDbContextMethod = providerType.GetMethod(nameof(IShardingDbContextProvider<TDbContext>.GetDbContext))!;
                dbContext = (DbContext)getDbContextMethod.Invoke(provider, null)!;
                
                _dbContexts[dbContextType] = dbContext;
            }

            return (TDbContext)dbContext;
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var personId = _personContextProvider.GetContext().PersonId;
            var now = DateTime.UtcNow;
            var totalChanges = 0;

            foreach (var dbContext in _dbContexts.Values)
            {
                // Process audit and soft delete logic
                foreach (var entry in dbContext.ChangeTracker.Entries())
                {
                    if (entry.Entity is IAuditEntity auditEntity)
                    {
                        ApplySoftDelete(entry, auditEntity, personId, now);
                        ApplyAuditTracking(entry, auditEntity, personId, now);
                    }
                }

                totalChanges += await dbContext.SaveChangesAsync(cancellationToken);
            }

            return totalChanges;
        }

        private void ApplySoftDelete(Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry entry, IAuditEntity auditEntity, int? personId, DateTime now)
        {
            if (entry.State == EntityState.Deleted)
            {
                entry.State = EntityState.Modified;
                auditEntity.IsDeleted = true;
                auditEntity.UpdatedBy = personId;
                auditEntity.UpdatedAt = now;
            }
        }

        private void ApplyAuditTracking(Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry entry, IAuditEntity auditEntity, int? personId, DateTime now)
        {
            if (entry.State == EntityState.Added)
            {
                auditEntity.CreatedBy = personId;
                auditEntity.CreatedAt = now;
                auditEntity.IsDeleted = false;
            }
            else if (entry.State == EntityState.Modified)
            {
                auditEntity.UpdatedBy = personId;
                auditEntity.UpdatedAt = now;
            }
        }

        public async Task BeginTransactionAsync()
        {
            if (_currentTransaction != null)
            {
                throw new InvalidOperationException("A transaction is already in progress.");
            }

            // Begin transaction on the first DbContext
            var firstDbContext = _dbContexts.Values.FirstOrDefault();
            if (firstDbContext != null)
            {
                _currentTransaction = await firstDbContext.Database.BeginTransactionAsync();
            }
        }

        public async Task CommitTransactionAsync()
        {
            if (_currentTransaction == null)
            {
                throw new InvalidOperationException("No transaction in progress.");
            }

            try
            {
                await SaveChangesAsync();
                await _currentTransaction.CommitAsync();
            }
            catch
            {
                await RollbackTransactionAsync();
                throw;
            }
            finally
            {
                if (_currentTransaction != null)
                {
                    await _currentTransaction.DisposeAsync();
                    _currentTransaction = null;
                }
            }
        }

        public async Task RollbackTransactionAsync()
        {
            if (_currentTransaction == null)
            {
                throw new InvalidOperationException("No transaction in progress.");
            }

            try
            {
                await _currentTransaction.RollbackAsync();
            }
            finally
            {
                if (_currentTransaction != null)
                {
                    await _currentTransaction.DisposeAsync();
                    _currentTransaction = null;
                }
            }
        }

        // Internal method to register a DbContext instance
        internal void RegisterDbContext<TDbContext>(TDbContext dbContext) where TDbContext : DbContext
        {
            _dbContexts[typeof(TDbContext)] = dbContext;
        }
    }
}
