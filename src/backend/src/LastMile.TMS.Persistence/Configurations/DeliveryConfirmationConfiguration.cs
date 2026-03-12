using LastMile.TMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LastMile.TMS.Persistence.Configurations;

public class DeliveryConfirmationConfiguration : IEntityTypeConfiguration<DeliveryConfirmation>
{
    public void Configure(EntityTypeBuilder<DeliveryConfirmation> builder)
    {
        builder.Property(dc => dc.ReceivedBy).HasMaxLength(200);
        builder.Property(dc => dc.DeliveryLocation).HasMaxLength(500);
        builder.Property(dc => dc.SignatureImage).HasMaxLength(500);
        builder.Property(dc => dc.Photo).HasMaxLength(500);

        builder.Property(dc => dc.DeliveryLocationCoords)
            .HasColumnType("geometry (point)");

        // One-to-one with Parcel
        builder.HasOne(dc => dc.Parcel)
            .WithOne(p => p.DeliveryConfirmation)
            .HasForeignKey<DeliveryConfirmation>(dc => dc.ParcelId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}