using LastMile.TMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LastMile.TMS.Persistence.Configurations;

public class ZoneConfiguration : IEntityTypeConfiguration<Zone>
{
    public void Configure(EntityTypeBuilder<Zone> builder)
    {
        builder.Property(z => z.Code).HasMaxLength(20).IsRequired();
        builder.Property(z => z.Name).HasMaxLength(100).IsRequired();

        builder.HasIndex(z => z.Code).IsUnique();
    }
}