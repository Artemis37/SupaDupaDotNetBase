using System.Linq.Expressions;
using System.Threading;
using Microsoft.EntityFrameworkCore;
using Shared.Application.Interfaces;
using Shared.Infrastructure.Context;
using Vehicle.Domain.Models;
using Vehicle.Infrastructure.Data.EntityConfigurations;

namespace Vehicle.Infrastructure.Data
{
    public class ShardingDbContext : DbContext
    {
        private readonly IPersonContextProvider _personContextProvider;

        public ShardingDbContext(
            DbContextOptions<ShardingDbContext> options,
            IPersonContextProvider personContextProvider) 
            : base(options)
        {
            _personContextProvider = personContextProvider;
            // Store provider reference for query filters
            QueryFilterHelper.SetProvider(personContextProvider);
        }

        public DbSet<Person> Persons { get; set; }
        public DbSet<Domain.Models.Vehicle> Vehicles { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ConfigurePerson();
            modelBuilder.ConfigureVehicle();

            // Store reference to personContextProvider for query filters
            var personContextProviderField = typeof(ShardingDbContext).GetField(
                "_personContextProvider", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            // Add query filters for IPersonEntity and IAuditEntity
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                var clrType = entityType.ClrType;
                
                // Filter by PersonId for entities implementing IPersonEntity
                if (typeof(IPersonEntity).IsAssignableFrom(clrType))
                {
                    // Get the PersonId property from the entity type
                    var personIdProperty = clrType.GetProperty("PersonId");
                    if (personIdProperty == null)
                    {
                        throw new InvalidOperationException($"Entity {clrType.Name} implements IPersonEntity but does not have a PersonId property.");
                    }

                    var parameter = Expression.Parameter(clrType, "e");
                    var property = Expression.Property(parameter, personIdProperty);
                    
                    // Create expression to get PersonId from PersonContextProvider
                    // We'll use a compiled method that accesses the provider
                    var getPersonIdMethod = typeof(QueryFilterHelper).GetMethod(
                        nameof(QueryFilterHelper.GetCurrentPersonId))!;
                    var getPersonIdCall = Expression.Call(getPersonIdMethod);
                    var personIdNullable = Expression.Convert(getPersonIdCall, typeof(int?));
                    
                    // Create condition: if PersonId is null, return true (no filter), otherwise filter by PersonId
                    var hasValueProperty = Expression.Property(personIdNullable, "HasValue");
                    var valueProperty = Expression.Property(personIdNullable, "Value");
                    var condition = Expression.Condition(
                        hasValueProperty,
                        Expression.Equal(property, Expression.Convert(valueProperty, typeof(int))),
                        Expression.Constant(true)); // If no PersonId set, don't filter
                    
                    var lambda = Expression.Lambda(condition, parameter);
                    modelBuilder.Entity(clrType).HasQueryFilter(lambda);
                }

                // Filter by IsDeleted == false for entities implementing IAuditEntity
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
    }

    // Helper class for query filters to access PersonContext
    internal static class QueryFilterHelper
    {
        private static readonly AsyncLocal<IPersonContextProvider?> _currentProvider = new();

        public static void SetProvider(IPersonContextProvider provider)
        {
            _currentProvider.Value = provider;
        }

        public static int? GetCurrentPersonId()
        {
            var provider = _currentProvider.Value;
            return provider?.GetContext().PersonId;
        }
    }
}
