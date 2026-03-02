using MechanicShop.Domain.RepairTasks.Parts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MechanicShop.Infrastructure.Data.Configuration;

public sealed class PartConfiguration : IEntityTypeConfiguration<Part>
{
    public void Configure(EntityTypeBuilder<Part> builder)
    {
      builder.ToTable(Tables.Parts , t =>
      {
          t.HasCheckConstraint("CK_Part_Cost_Range",
              "\"Cost\" >= 1 AND \"Cost\" <= 100000");

          t.HasCheckConstraint("CK_Part_Quantity_Range",
              "\"Quantity\" >= 1 AND \"Quantity\" <= 100");
      });

      builder.HasKey(p => p.Id);

      builder.Property(p => p.Id)
              .ValueGeneratedNever();

      builder.Property(p => p.Name)
              .HasMaxLength(100)
              .IsRequired();

      builder.Property(p => p.Cost)
              .HasPrecision(8, 2)
              .IsRequired();

      builder.Property(p => p.Quantity)
              .IsRequired();
    }
}