using MechanicShop.Domain.Employees;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MechanicShop.Infrastructure.Data.Configuration;

public sealed class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
{
  public void Configure(EntityTypeBuilder<Employee> builder)
  {
    builder.ToTable(Tables.Employees);

    builder.HasKey(e => e.Id);

    builder.Property(e => e.FirstName)
            .HasMaxLength(50)
            .IsRequired();
    
    builder.Property(e => e.LastName)
            .HasMaxLength(50)
            .IsRequired();
    
    builder.Property(e => e.Role)
            .HasConversion<string>()
            .IsRequired();
  }
}