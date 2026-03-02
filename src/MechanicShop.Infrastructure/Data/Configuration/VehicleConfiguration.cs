using MechanicShop.Domain.Customers.Vehicles;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MechanicShop.Infrastructure.Data.Configuration;

public class VehicleConfiguration : IEntityTypeConfiguration<Vehicle>
{
  public void Configure(EntityTypeBuilder<Vehicle> builder)
  {
    builder.ToTable(Tables.Vehicles);

    builder.HasKey(v => v.Id);

    builder.Property(v => v.Id).ValueGeneratedNever();

    builder.Property(v => v.Make)
            .HasMaxLength(50)
            .IsRequired();

    builder.Property(v => v.Model)
            .HasMaxLength(50)
            .IsRequired();
    
    builder.Property(v => v.LicensePlate)
            .HasMaxLength(10)
            .IsRequired();
    
    builder.Property(v => v.Year)
            .IsRequired();

    builder.HasOne(v => v.Customer)
            .WithMany(c => c.Vehicles)
            .HasForeignKey(v => v.CustomerId);
  }
}