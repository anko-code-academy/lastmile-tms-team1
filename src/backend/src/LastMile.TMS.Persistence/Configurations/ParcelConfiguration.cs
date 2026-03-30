using LastMile.TMS.Domain.Entities;
using LastMile.TMS.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LastMile.TMS.Persistence.Configurations;

public class ParcelConfiguration : IEntityTypeConfiguration<Parcel>
{
    public void Configure(EntityTypeBuilder<Parcel> builder)
    {
        builder.ToTable("Parcel");

        // Tracking number
        builder.Property(p => p.TrackingNumber)
            .HasMaxLength(20)
            .IsRequired();
        builder.HasIndex(p => p.TrackingNumber).IsUnique();

        builder.Property(p => p.Description).HasMaxLength(500);
        builder.Property(p => p.ServiceType).HasConversion<string>().HasMaxLength(20);
        builder.Property(p => p.Status).HasConversion<string>().HasMaxLength(30);

        // Weight & Dimensions
        builder.Property(p => p.Weight).HasPrecision(10, 3);
        builder.Property(p => p.WeightUnit).HasConversion<string>().HasMaxLength(2);
        builder.Property(p => p.Length).HasPrecision(10, 3);
        builder.Property(p => p.Width).HasPrecision(10, 3);
        builder.Property(p => p.Height).HasPrecision(10, 3);
        builder.Property(p => p.DimensionUnit).HasConversion<string>().HasMaxLength(2);

        // Value
        builder.Property(p => p.DeclaredValue).HasPrecision(18, 2);
        builder.Property(p => p.Currency).HasMaxLength(3).IsRequired();

        // Delivery
        builder.Property(p => p.DeliveryAttempts).HasDefaultValue(0);
        builder.Property(p => p.ParcelType).HasMaxLength(100);

        // Indexes
        builder.HasIndex(p => p.Status);
        builder.HasIndex(p => p.EstimatedDeliveryDate);

        // Relationships
        builder.HasOne(p => p.ShipperAddress)
            .WithMany(a => a.ShipperParcels)
            .HasForeignKey(p => p.ShipperAddressId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.RecipientAddress)
            .WithMany(a => a.RecipientParcels)
            .HasForeignKey(p => p.RecipientAddressId)
            .OnDelete(DeleteBehavior.Restrict);

        // Zone relationship (one-way - no inverse navigation in Zone)
        builder.HasOne(p => p.Zone)
            .WithMany()
            .HasForeignKey(p => p.ZoneId)
            .OnDelete(DeleteBehavior.SetNull);

        // Soft delete
        builder.Property(p => p.IsDeleted).HasDefaultValue(false);
        builder.Property(p => p.DeletedAt);
        builder.Property(p => p.DeletedBy).HasMaxLength(256);
    }
}