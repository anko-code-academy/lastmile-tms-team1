using LastMile.TMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LastMile.TMS.Persistence.Configurations;

public class ShiftScheduleConfiguration : IEntityTypeConfiguration<ShiftSchedule>
{
    public void Configure(EntityTypeBuilder<ShiftSchedule> builder)
    {
        builder.ToTable("ShiftSchedules");

        builder.Property(s => s.DayOfWeek)
            .IsRequired();

        builder.Property(s => s.OpenTime)
            .IsRequired();

        builder.Property(s => s.CloseTime)
            .IsRequired();

        // Both FKs are nullable — XOR enforced at DB via check constraint
        builder.Property(s => s.DriverId)
            .IsRequired(false);

        builder.Property(s => s.DepotId)
            .IsRequired(false);

        builder.HasOne(s => s.Driver)
            .WithMany(d => d.ShiftSchedules)
            .HasForeignKey(s => s.DriverId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(s => s.Depot)
            .WithMany(d => d.ShiftSchedules)
            .HasForeignKey(s => s.DepotId)
            .OnDelete(DeleteBehavior.Cascade);

        // Soft delete
        builder.Property(s => s.IsDeleted).HasDefaultValue(false);
        builder.Property(s => s.DeletedAt);
        builder.Property(s => s.DeletedBy).HasMaxLength(256);
    }
}