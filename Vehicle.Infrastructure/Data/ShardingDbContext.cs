using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Shared.Application.Interfaces;
using Shared.Application.Context;
using Vehicle.Domain.Models;
using Vehicle.Infrastructure.Data.EntityConfigurations;

namespace Vehicle.Infrastructure.Data
{
    public class ShardingDbContext : DbContext, IUnitOfWork
    {
        private IDbContextTransaction? _currentTransaction;

        public ShardingDbContext(DbContextOptions<ShardingDbContext> options) 
            : base(options)
        {
        }

        public DbSet<Person> Persons { get; set; }
        public DbSet<Domain.Models.Vehicle> Vehicles { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ConfigurePerson();
            modelBuilder.ConfigureVehicle();
            
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                var clrType = entityType.ClrType;
                
                if (typeof(IPersonEntity).IsAssignableFrom(clrType))
                {
                    var parameter = Expression.Parameter(clrType, "e");
                    var property = Expression.Property(parameter, "PersonId");
                    var personContext = PersonContextProvider.Current;
                    var personIdExpression = Expression.Property(Expression.Constant(personContext), nameof(PersonContext.PersonId));
                    
                    Expression nullableProperty = property.Type == typeof(int) 
                        ? Expression.Convert(property, typeof(int?)) 
                        : (Expression)property;
                    
                    // Check if PersonId is null - if so, return true (no filter), otherwise apply the filter
                    var isNullCheck = Expression.Equal(personIdExpression, Expression.Constant(null, typeof(int?)));
                    var equalityFilter = Expression.Equal(nullableProperty, personIdExpression);
                    var trueConstant = Expression.Constant(true, typeof(bool));
                    var filter = Expression.Condition(isNullCheck, trueConstant, equalityFilter);
                    
                    var lambda = Expression.Lambda(filter, parameter);
                    modelBuilder.Entity(clrType).HasQueryFilter(lambda);
                }

                if (typeof(IAuditEntity).IsAssignableFrom(clrType))
                {
                    var parameter = Expression.Parameter(clrType, "e");
                    var property = Expression.Property(parameter, nameof(IAuditEntity.IsDeleted));
                    var constant = Expression.Constant(false, typeof(bool));
                    var filter = Expression.Equal(property, constant);
                    var lambda = Expression.Lambda(filter, parameter);
                    modelBuilder.Entity(clrType).HasQueryFilter(lambda);
                }
            }
        }

        public TDbContext GetDbContext<TDbContext>() where TDbContext : DbContext
        {
            if (typeof(TDbContext) != typeof(ShardingDbContext))
            {
                throw new InvalidOperationException($"Only {nameof(ShardingDbContext)} is supported. Requested type: {typeof(TDbContext).Name}");
            }

            return (TDbContext)(object)this;
        }

        public new async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var personContext = PersonContextProvider.Current;
            var personId = personContext?.PersonId;
            var now = DateTime.UtcNow;

            // Process audit and soft delete logic
            foreach (var entry in ChangeTracker.Entries())
            {
                if (entry.Entity is IAuditEntity auditEntity)
                {
                    ApplySoftDelete(entry, auditEntity, personId, now);
                    ApplyAuditTracking(entry, auditEntity, personId, now);
                }
            }
            
            return await base.SaveChangesAsync(cancellationToken);
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

            _currentTransaction = await Database.BeginTransactionAsync();
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
    }
}
