using LastMile.TMS.Domain.Entities;
using LastMile.TMS.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LastMile.TMS.Persistence.Configurations;

public class RouteStopConfiguration : IEntityTypeConfiguration<RouteStop>
{
    public void Configure(EntityTypeBuilder<RouteStop> builder)
    {
        builder.ToTable("RouteStops");

        builder.Property(s => s.SequenceNumber)
            .IsRequired();

        builder.Property(s => s.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(s => s.EstimatedServiceMinutes)
            .HasDefaultValue(0);

        builder.Property(s => s.AccessInstructions)
            .HasMaxLength(1000);

        builder.Property(s => s.Street1)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(s => s.GeoLocation)
            .HasColumnType("geometry (point)");

        // Unique composite index: one sequence number per route
        builder.HasIndex(s => new { s.RouteId, s.SequenceNumber }).IsUnique();

        // Spatial index for proximity queries
        builder.HasIndex(s => s.GeoLocation);

        // Relationships
        builder.HasOne(s => s.Route)
            .WithMany(r => r.RouteStops)
            .HasForeignKey(s => s.RouteId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(s => s.Parcels)
            .WithOne(p => p.RouteStop)
            .HasForeignKey(p => p.RouteStopId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Navigation(s => s.Parcels)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
