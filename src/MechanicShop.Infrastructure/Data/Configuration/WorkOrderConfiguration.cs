using MechanicShop.Domain.WorkOrders;
using MechanicShop.Domain.WorkOrders.Billing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MechanicShop.Infrastructure.Data.Configuration;

public sealed class WorkOrderConfiguration : IEntityTypeConfiguration<WorkOrder>
{
  public void Configure(EntityTypeBuilder<WorkOrder> builder)
  {
    builder.ToTable(Tables.WorkOrders);

    builder.HasKey(wo => wo.Id);

    builder.Property(wo => wo.LaborId)
            .IsRequired();
    
    builder.Property(w => w.State)
            .HasConversion<string>()
            .IsRequired();
    
    builder.Property(w => w.Spot)
            .HasConversion<string>()
            .IsRequired();
    
    builder.Property(wo => wo.StartAtUtc)
            .IsRequired();

    builder.Property(wo => wo.EndAtUtc)
            .IsRequired();

    builder.Property(wo => wo.Tax)
            .HasPrecision(18 , 2);

    builder.Property(w => w.Discount)
            .HasPrecision(18, 2);

    builder.Ignore(wo => wo.Total);
    
    builder.Ignore(wo => wo.TotalLaborCost);
    
    builder.Ignore(wo => wo.TotalPartsCost);
    
    builder.Ignore(wo => wo.IsEditable);

    builder.HasOne(wo => wo.Labor)
            .WithMany()
            .HasForeignKey(w => w.LaborId)
            .IsRequired();

    builder.HasOne(wo => wo.Vehicle)
            .WithMany()
            .HasForeignKey(w => w.VehicleId);

    builder.HasMany(wo => wo.RepairTasks)
            .WithMany()
            .UsingEntity(j => j.ToTable("WorkOrderRepairTasks"));
    
    builder.HasOne(wo => wo.Invoice)
            .WithOne(i => i.WorkOrder)
            .HasForeignKey<Invoice>(i => i.WorkOrderId)
            .OnDelete(DeleteBehavior.Restrict);

    builder.Navigation(wo => wo.RepairTasks)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

    builder.HasIndex(wo => wo.LaborId);
    
    builder.HasIndex(wo => wo.VehicleId);
    
    builder.HasIndex(wo => wo.State);
    
    builder.HasIndex(wo => new { wo.StartAtUtc , wo.EndAtUtc });
  }
}