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

        builder.HasIndex(s => new { s.DriverId, s.DayOfWeek })
            .IsUnique();

        builder.HasOne(s => s.Driver)
            .WithMany(d => d.ShiftSchedules)
            .HasForeignKey(s => s.DriverId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}