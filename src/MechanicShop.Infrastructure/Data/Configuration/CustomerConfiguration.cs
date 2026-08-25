using MechanicShop.Domain.Customers;
using MechanicShop.Domain.Common.ValueObjects.EmailAddress;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MechanicShop.Domain.Common.ValueObjects.PhoneNumber;

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
            .HasConversion(
              phone => phone!.Value,
              phone => PhoneNumber.Create(phone).Value
            )
            .HasMaxLength(11)
            .IsRequired(false);

    builder.Property(e => e.Email)
            .HasConversion(
              email => email!.Value,
              value => EmailAddress.Create(value).Value)
            .IsRequired(false)
            .HasMaxLength(100);

    builder.Navigation(e => e.Vehicles)
      .UsePropertyAccessMode(PropertyAccessMode.Field);
  }
}
