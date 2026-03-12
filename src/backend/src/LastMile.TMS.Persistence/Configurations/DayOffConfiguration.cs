using LastMile.TMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LastMile.TMS.Persistence.Configurations;

public class DayOffConfiguration : IEntityTypeConfiguration<DayOff>
{
    public void Configure(EntityTypeBuilder<DayOff> builder)
    {
        builder.ToTable("DaysOff");

        builder.Property(d => d.Date)
            .IsRequired();

        builder.HasIndex(d => new { d.DriverId, d.Date });

        builder.HasOne(d => d.Driver)
            .WithMany(d => d.DaysOff)
            .HasForeignKey(d => d.DriverId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}