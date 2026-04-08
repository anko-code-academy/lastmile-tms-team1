using LastMile.TMS.Domain.Entities;
using LastMile.TMS.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LastMile.TMS.Persistence.Configurations;

public class RouteConfiguration : IEntityTypeConfiguration<Route>
{
    public void Configure(EntityTypeBuilder<Route> builder)
    {
        builder.Property(r => r.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(r => r.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(r => r.PlannedStartTime)
            .IsRequired();

        builder.Property(r => r.TotalDistanceKm)
            .HasPrecision(10, 2);

        builder.Property(r => r.TotalParcelCount)
            .HasDefaultValue(0);

        builder.HasIndex(r => r.Status);
        builder.HasIndex(r => r.PlannedStartTime);

        builder.HasOne(r => r.Vehicle)
            .WithMany()
            .HasForeignKey(r => r.VehicleId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(r => r.Driver)
            .WithMany(d => d.Routes)
            .HasForeignKey(r => r.DriverId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(r => r.DriverId);

        builder.HasOne(r => r.Zone)
            .WithMany()
            .HasForeignKey(r => r.ZoneId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(r => r.ZoneId);

        builder.Navigation(r => r.VehicleJourneys)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Navigation(r => r.RouteStops)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
