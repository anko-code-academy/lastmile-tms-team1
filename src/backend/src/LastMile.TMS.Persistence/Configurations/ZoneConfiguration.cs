using LastMile.TMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NetTopologySuite;
using NetTopologySuite.Geometries;

namespace LastMile.TMS.Persistence.Configurations;

public class ZoneConfiguration : IEntityTypeConfiguration<Zone>
{
    private static readonly GeometryFactory GeometryFactory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);

    public void Configure(EntityTypeBuilder<Zone> builder)
    {
        builder.ToTable("Zones");

        builder.Property(z => z.Name)
           .IsRequired()
           .HasMaxLength(200);

        builder.Property(z => z.IsActive)
           .IsRequired();

        builder.Property(z => z.BoundaryGeometry)
           .IsRequired()
           .HasColumnType("geometry (polygon)");

        builder.HasIndex(z => z.BoundaryGeometry)
           .HasMethod("GIST");

        builder.HasOne(z => z.Depot)
          .WithMany(d => d.Zones)
          .HasForeignKey(z => z.DepotId)
          .OnDelete(DeleteBehavior.Cascade);
    }
}
