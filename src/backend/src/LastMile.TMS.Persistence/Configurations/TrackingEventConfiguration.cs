using LastMile.TMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LastMile.TMS.Persistence.Configurations;

public class TrackingEventConfiguration : IEntityTypeConfiguration<TrackingEvent>
{
    public void Configure(EntityTypeBuilder<TrackingEvent> builder)
    {
        builder.Property(te => te.EventType).HasConversion<string>().HasMaxLength(30);
        builder.Property(te => te.Description).HasMaxLength(500);
        builder.Property(te => te.LocationCity).HasMaxLength(100);
        builder.Property(te => te.LocationState).HasMaxLength(100);
        builder.Property(te => te.LocationCountry).HasMaxLength(2);
        builder.Property(te => te.Operator).HasMaxLength(100);
        builder.Property(te => te.DelayReason).HasConversion<string>().HasMaxLength(30);

        builder.HasIndex(te => te.Timestamp);
        builder.HasIndex(te => new { te.ParcelId, te.Timestamp });

        builder.HasOne(te => te.Parcel)
            .WithMany(p => p.TrackingEvents)
            .HasForeignKey(te => te.ParcelId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}