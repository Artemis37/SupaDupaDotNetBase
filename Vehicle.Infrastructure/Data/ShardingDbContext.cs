using Microsoft.EntityFrameworkCore;
using Vehicle.Domain.Models;
using Vehicle.Infrastructure.Data.EntityConfigurations;

namespace Vehicle.Infrastructure.Data
{
    public class ShardingDbContext : DbContext
    {
        public ShardingDbContext(DbContextOptions<ShardingDbContext> options) : base(options)
        {
        }

        public DbSet<Person> Persons { get; set; }
        public DbSet<Domain.Models.Vehicle> Vehicles { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ConfigurePerson();
            modelBuilder.ConfigureVehicle();
        }
    }
}
