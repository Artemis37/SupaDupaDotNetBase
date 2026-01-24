using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vehicle.Domain.Models;

namespace Vehicle.Infrastructure.Data.EntityConfigurations
{
    public static class PersonMasterConfiguration
    {
        public static void ConfigurePersonMaster(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PersonMaster>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.Property(e => e.ShardId).IsRequired();
                entity.Property(e => e.Username).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Password).IsRequired().HasMaxLength(500); // Hashed password
                entity.Property(e => e.CreatedAt).IsRequired();
                entity.Property(e => e.UpdatedAt);
                entity.Property(e => e.CreatedBy).HasMaxLength(255);
                entity.Property(e => e.UpdatedBy).HasMaxLength(255);

                // Create index on Username for faster lookups
                entity.HasIndex(e => e.Username).IsUnique();
            });
        }
    }
}
