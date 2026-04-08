using LastMile.TMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NpgsqlTypes;

namespace LastMile.TMS.Persistence.Configurations;

public class AddressConfiguration : IEntityTypeConfiguration<Address>
{
    public void Configure(EntityTypeBuilder<Address> builder)
    {
        builder.ToTable("Addresses");

        // Full-text search: separate tsvectors for recipient name and address
        builder.Property<NpgsqlTsVector>("RecipientNameSearchVector")
            .HasComputedColumnSql(
                @"to_tsvector('english', coalesce(""ContactName"", ''))",
                stored: true);
        builder.HasIndex("RecipientNameSearchVector").HasMethod("GIN");

        builder.Property<NpgsqlTsVector>("AddressSearchVector")
            .HasComputedColumnSql(
                @"to_tsvector('english', coalesce(""Street1"", '') || ' ' || coalesce(""City"", '') || ' ' || coalesce(""PostalCode"", ''))",
                stored: true);
        builder.HasIndex("AddressSearchVector").HasMethod("GIN");

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
