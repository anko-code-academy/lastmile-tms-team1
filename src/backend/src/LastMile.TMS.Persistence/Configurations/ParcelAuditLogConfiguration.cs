using LastMile.TMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LastMile.TMS.Persistence.Configurations;

public class ParcelAuditLogConfiguration : IEntityTypeConfiguration<ParcelAuditLog>
{
    public void Configure(EntityTypeBuilder<ParcelAuditLog> builder)
    {
        builder.HasIndex(pal => pal.ParcelId);
        builder.HasIndex(pal => new { pal.ParcelId, pal.CreatedAt });

        builder.HasOne(pal => pal.Parcel)
            .WithMany(p => p.AuditLogs)
            .HasForeignKey(pal => pal.ParcelId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(pal => pal.PropertyName).HasMaxLength(256);
        builder.Property(pal => pal.OldValue);
        builder.Property(pal => pal.NewValue);
        builder.Property(pal => pal.ChangedBy).HasMaxLength(256);

        // Soft delete
        builder.Property(pal => pal.IsDeleted).HasDefaultValue(false);
        builder.Property(pal => pal.DeletedAt);
        builder.Property(pal => pal.DeletedBy).HasMaxLength(256);
    }
}
