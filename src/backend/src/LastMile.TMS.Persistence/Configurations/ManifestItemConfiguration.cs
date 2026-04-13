using LastMile.TMS.Domain.Entities;
using LastMile.TMS.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LastMile.TMS.Persistence.Configurations;

public class ManifestItemConfiguration : IEntityTypeConfiguration<ManifestItem>
{
    public void Configure(EntityTypeBuilder<ManifestItem> builder)
    {
        builder.ToTable("ManifestItems");

        builder.Property(mi => mi.TrackingNumber)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(mi => mi.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.HasIndex(mi => new { mi.ManifestId, mi.TrackingNumber }).IsUnique();

        builder.HasOne(mi => mi.Manifest)
            .WithMany(m => m.Items)
            .HasForeignKey(mi => mi.ManifestId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(mi => mi.Parcel)
            .WithMany()
            .HasForeignKey(mi => mi.ParcelId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
