using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Shared.Application.Interfaces;
using Shared.Application.Context;
using Vehicle.Domain.Models;
using Vehicle.Infrastructure.Data.EntityConfigurations;

namespace Vehicle.Infrastructure.Data
{
    public class ShardingDbContext : DbContext
    {
        private readonly PersonContext _personContext;

        public ShardingDbContext(
            DbContextOptions<ShardingDbContext> options,
            PersonContext personContext) 
            : base(options)
        {
            _personContext = personContext;
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
                    var personIdExpression = Expression.Property(Expression.Constant(_personContext), nameof(PersonContext.PersonId));
                    var filter = Expression.Equal(property, personIdExpression);
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
    }
}
