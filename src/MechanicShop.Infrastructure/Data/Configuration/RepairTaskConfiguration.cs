using MechanicShop.Domain.RepairTasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MechanicShop.Infrastructure.Data.Configuration;

public sealed class RepairTaskConfiguration 
    : IEntityTypeConfiguration<RepairTask>
{
    public void Configure(EntityTypeBuilder<RepairTask> builder)
    {
        builder.ToTable(Tables.RepairTasks , t =>
        {
            t.HasCheckConstraint("CK_RepairTask_LaborCost_Range",
                "\"LaborCost\" >= 1 AND \"LaborCost\" <= 10000");
        });

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Id)
                .ValueGeneratedNever();

        builder.Property(r => r.Name)
                .HasColumnType("CITEXT")
                .HasMaxLength(100)
                .IsRequired();

        builder.Property(r => r.LaborCost)
                .HasPrecision(7, 2) 
                .IsRequired();

        builder.Property(r => r.EstimatedDurationInMins)
                .HasConversion<string>() 
                .IsRequired();

        builder.HasMany(rt => rt.Parts)
                .WithOne()
                .HasForeignKey("RepairTaskId")
                .OnDelete(DeleteBehavior.Cascade);
        
        builder.Navigation(rt => rt.Parts)
                .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}