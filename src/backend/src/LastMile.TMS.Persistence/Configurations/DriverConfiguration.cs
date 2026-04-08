using LastMile.TMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LastMile.TMS.Persistence.Configurations;

public class DriverConfiguration : IEntityTypeConfiguration<Driver>
{
    public void Configure(EntityTypeBuilder<Driver> builder)
    {
        builder.ToTable("Drivers");

        builder.Property(d => d.LicenseNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(d => d.LicenseExpiryDate)
            .IsRequired();

        builder.Property(d => d.Photo)
            .HasMaxLength(500);

        builder.HasIndex(d => d.LicenseNumber);

        // Relationships
        builder.HasMany(d => d.ShiftSchedules)
            .WithOne(s => s.Driver)
            .HasForeignKey(s => s.DriverId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(d => d.DaysOff)
            .WithOne(d => d.Driver)
            .HasForeignKey(d => d.DriverId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(d => d.User)
            .WithMany()
            .HasForeignKey(d => d.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(d => d.UserId).IsUnique();

        // Soft delete
        builder.Property(d => d.IsDeleted).HasDefaultValue(false);
        builder.Property(d => d.DeletedAt);
        builder.Property(d => d.DeletedBy).HasMaxLength(256);
    }
}
