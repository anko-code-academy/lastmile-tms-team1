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

        // Phone - optional
        builder.Property(u => u.Phone)
            .HasMaxLength(20);

        // Password hash
        builder.Property(u => u.PasswordHash)
            .HasMaxLength(500);

        // Status
        builder.Property(u => u.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        // IsActive
        builder.Property(u => u.IsActive)
            .IsRequired();

        // Indexes
        builder.HasIndex(u => u.IsActive);

        // Relationships
        builder.HasOne(u => u.Zone)
            .WithMany()
            .HasForeignKey(u => u.ZoneId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(u => u.Depot)
            .WithMany()
            .HasForeignKey(u => u.DepotId)
            .OnDelete(DeleteBehavior.SetNull);

        // UserRoles navigation
        builder.HasMany(u => u.UserRoles)
            .WithOne(ur => ur.User)
            .HasForeignKey(ur => ur.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}