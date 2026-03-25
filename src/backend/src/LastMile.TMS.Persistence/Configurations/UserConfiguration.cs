using LastMile.TMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LastMile.TMS.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        // Name properties
        builder.Property(u => u.FirstName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(u => u.LastName)
            .IsRequired()
            .HasMaxLength(100);

        // Email - unique
        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(255);
        builder.HasIndex(u => u.Email).IsUnique();

        // Phone (PhoneNumber) - optional
        builder.Property(u => u.PhoneNumber)
            .HasMaxLength(20);

        // Username (required by Identity)
        builder.Property(u => u.UserName)
            .IsRequired()
            .HasMaxLength(255);
        builder.HasIndex(u => u.UserName).IsUnique();

        // Password hash
        builder.Property(u => u.PasswordHash)
            .HasMaxLength(500);

        // Security stamp for identity
        builder.Property(u => u.SecurityStamp)
            .HasMaxLength(500);

        // Concurrency stamp
        builder.Property(u => u.ConcurrencyStamp)
            .HasMaxLength(500);

        // Status
        builder.Property(u => u.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        // Indexes
        builder.HasIndex(u => u.Status);

        // Relationships
        builder.HasOne(u => u.Zone)
            .WithMany()
            .HasForeignKey(u => u.ZoneId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(u => u.Depot)
            .WithMany()
            .HasForeignKey(u => u.DepotId)
            .OnDelete(DeleteBehavior.SetNull);

        // Role relationship (1:1)
        builder.HasOne(u => u.Role)
            .WithMany(r => r.Users)
            .HasForeignKey(u => u.RoleId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(u => u.RoleId);

        // Soft delete
        builder.Property(u => u.IsDeleted).HasDefaultValue(false);
        builder.Property(u => u.DeletedAt);
        builder.Property(u => u.DeletedBy).HasMaxLength(256);

        // System admin flag
        builder.Property(u => u.IsSystemAdmin).HasDefaultValue(false);
    }
}