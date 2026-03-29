using LastMile.TMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LastMile.TMS.Persistence.Configurations;

public class DepotConfiguration : IEntityTypeConfiguration<Depot>
{
    public void Configure(EntityTypeBuilder<Depot> builder)
    {
        builder.ToTable("Depots");

        builder.Property(d => d.Name)
           .IsRequired()
           .HasMaxLength(200);

        builder.Property(d => d.IsActive)
            .IsRequired();

        builder.HasOne(d => d.Address)
           .WithMany()
           .HasForeignKey(d => d.AddressId)
           .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(d => d.Zones)
           .WithOne(z => z.Depot)
           .HasForeignKey(z => z.DepotId)
           .OnDelete(DeleteBehavior.Cascade);

        // Soft delete
        builder.Property(d => d.IsDeleted).HasDefaultValue(false);
        builder.Property(d => d.DeletedAt);
        builder.Property(d => d.DeletedBy).HasMaxLength(256);
    }
}