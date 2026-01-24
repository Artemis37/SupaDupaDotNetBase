using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vehicle.Domain.Models;

namespace Vehicle.Infrastructure.Data.EntityConfigurations
{
    public static class VehicleConfiguration
    {
        public static void ConfigureVehicle(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Domain.Models.Vehicle>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.Property(e => e.PersonId).IsRequired();
                entity.Property(e => e.Type).IsRequired().HasConversion<int>();
                entity.Property(e => e.LicensePlate).IsRequired().HasMaxLength(50);

                // Configure relationship
                entity.HasOne(v => v.Person)
                    .WithMany()
                    .HasForeignKey(v => v.PersonId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
