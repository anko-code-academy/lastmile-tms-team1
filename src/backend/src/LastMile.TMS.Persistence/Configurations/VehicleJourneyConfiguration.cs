using LastMile.TMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LastMile.TMS.Persistence.Configurations;

public class VehicleJourneyConfiguration : IEntityTypeConfiguration<VehicleJourney>
{
    public void Configure(EntityTypeBuilder<VehicleJourney> builder)
    {
        builder.Property(vj => vj.StartMileageKm)
            .HasPrecision(10, 2);

        builder.Property(vj => vj.EndMileageKm)
            .HasPrecision(10, 2);

        builder.HasIndex(vj => vj.VehicleId);
        builder.HasIndex(vj => vj.RouteId);
        builder.HasIndex(vj => vj.StartTime);

        builder.HasOne(vj => vj.Route)
            .WithMany(r => r.VehicleJourneys)
            .HasForeignKey(vj => vj.RouteId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(vj => vj.Vehicle)
            .WithMany()
            .HasForeignKey(vj => vj.VehicleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(vj => vj.Route)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(vj => vj.Vehicle)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
