using LastMile.TMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LastMile.TMS.Persistence.Configurations;

public class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        builder.ToTable("Permissions");

        // Code - unique identifier
        builder.Property(p => p.Code)
            .IsRequired()
            .HasMaxLength(100);
        builder.HasIndex(p => p.Code).IsUnique();

        // Name
        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(200);

        // Description - optional
        builder.Property(p => p.Description)
            .HasMaxLength(500);

        // Module
        builder.Property(p => p.Module)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        // Scope
        builder.Property(p => p.Scope)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        // Relationships
        builder.HasMany(p => p.RolePermissions)
            .WithOne(rp => rp.Permission)
            .HasForeignKey(rp => rp.PermissionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}