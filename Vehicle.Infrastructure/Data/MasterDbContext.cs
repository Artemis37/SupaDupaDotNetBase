using Microsoft.EntityFrameworkCore;
using Vehicle.Domain.Models;
using Vehicle.Infrastructure.Data.EntityConfigurations;

namespace Vehicle.Infrastructure.Data
{
    public class MasterDbContext : DbContext
    {
        public MasterDbContext(DbContextOptions<MasterDbContext> options) : base(options)
        {
        }

        public DbSet<PersonMaster> PersonMasters { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ConfigurePersonMaster();
        }
    }
}
