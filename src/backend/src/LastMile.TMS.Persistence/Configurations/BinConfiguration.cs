using LastMile.TMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LastMile.TMS.Persistence.Configurations;

public class BinConfiguration : IEntityTypeConfiguration<Bin>
{
    public void Configure(EntityTypeBuilder<Bin> builder)
    {
        builder.ToTable("Bins");

        builder.Property(b => b.Label)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(b => b.Description)
            .HasMaxLength(500);

        builder.Property(b => b.Aisle)
            .IsRequired();

        builder.Property(b => b.Slot)
            .IsRequired();

        builder.Property(b => b.Capacity)
            .IsRequired();

        builder.Property(b => b.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.HasIndex(b => b.Label).IsUnique();
        builder.HasIndex(b => b.ZoneId);

        builder.HasOne(b => b.Zone)
            .WithMany(z => z.Bins)
            .HasForeignKey(b => b.ZoneId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
