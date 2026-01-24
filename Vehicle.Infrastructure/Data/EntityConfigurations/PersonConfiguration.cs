using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vehicle.Domain.Models;

namespace Vehicle.Infrastructure.Data.EntityConfigurations
{
    public static class PersonConfiguration
    {
        public static void ConfigurePerson(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Person>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
            });
        }
    }
}
