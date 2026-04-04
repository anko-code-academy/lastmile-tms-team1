using LastMile.TMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LastMile.TMS.Persistence.Configurations;

public class AddressConfiguration : IEntityTypeConfiguration<Address>
{
    public void Configure(EntityTypeBuilder<Address> builder)
    {
        builder.ToTable("Addresses");

        builder.Property(a => a.Street1).HasMaxLength(200).IsRequired();
        builder.Property(a => a.Street2).HasMaxLength(200);
        builder.Property(a => a.City).HasMaxLength(100).IsRequired();
        builder.Property(a => a.State).HasMaxLength(100).IsRequired();
        builder.Property(a => a.PostalCode).HasMaxLength(20).IsRequired();
        builder.Property(a => a.CountryCode).HasMaxLength(2).IsRequired();
        builder.Property(a => a.IsResidential).HasDefaultValue(false);
        builder.Property(a => a.ContactName).HasMaxLength(100);
        builder.Property(a => a.CompanyName).HasMaxLength(200);
        builder.Property(a => a.Phone).HasMaxLength(20);
        builder.Property(a => a.Email).HasMaxLength(200);

        // Geolocation (PostGIS Point)
        builder.Property(a => a.GeoLocation)
            .HasColumnType("geometry (point)");

        builder.HasIndex(a => a.PostalCode);
        builder.HasIndex(a => a.City);

        // Soft delete
        builder.Property(a => a.IsDeleted).HasDefaultValue(false);
        builder.Property(a => a.DeletedAt);
        builder.Property(a => a.DeletedBy).HasMaxLength(256);
    }
}