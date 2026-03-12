using LastMile.TMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LastMile.TMS.Persistence.Configurations;

public class ParcelWatcherConfiguration : IEntityTypeConfiguration<ParcelWatcher>
{
    public void Configure(EntityTypeBuilder<ParcelWatcher> builder)
    {
        builder.Property(pw => pw.Email).HasMaxLength(200).IsRequired();
        builder.Property(pw => pw.Name).HasMaxLength(100);

        builder.HasIndex(pw => pw.Email);

        // Many-to-many relationship
        builder.HasMany(pw => pw.Parcels)
            .WithMany(p => p.ParcelWatchers)
            .UsingEntity("ParcelWatcherParcels");
    }
}