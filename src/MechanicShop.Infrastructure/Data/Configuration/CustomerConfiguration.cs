using MechanicShop.Domain.Customers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MechanicShop.Infrastructure.Data.Configuration;

public sealed class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
  public void Configure(EntityTypeBuilder<Customer> builder)
  {
    builder.ToTable(Tables.Customers);

    builder.HasKey(e => e.Id);

    builder.Property(e => e.Name)
            .HasMaxLength(50)
            .IsRequired();

    builder.Property(e => e.PhoneNumber)
            .IsRequired();
    
    builder.Property(e => e.Email)
            .IsRequired()
            .HasMaxLength(100);
    
    builder.Navigation(e => e.Vehicles)
      .UsePropertyAccessMode(PropertyAccessMode.Field);
  }
}