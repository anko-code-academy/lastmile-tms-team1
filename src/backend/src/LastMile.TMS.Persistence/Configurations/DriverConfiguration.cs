using LastMile.TMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LastMile.TMS.Persistence.Configurations;

public class DriverConfiguration : IEntityTypeConfiguration<Driver>
{
    public void Configure(EntityTypeBuilder<Driver> builder)
    {
        builder.ToTable("Drivers");

        builder.Property(d => d.FirstName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(d => d.LastName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(d => d.Phone)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(d => d.Email)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(d => d.LicenseNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(d => d.LicenseExpiryDate)
            .IsRequired();

        builder.Property(d => d.Photo)
            .HasMaxLength(500);

        builder.Property(d => d.IsActive)
            .IsRequired();

        builder.HasIndex(d => d.Email).IsUnique();
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

        // Relationships - Zone and Depot
        builder.HasOne(d => d.Zone)
            .WithMany(z => z.Drivers)
            .HasForeignKey(d => d.ZoneId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(d => d.Depot)
            .WithMany(d => d.Drivers)
            .HasForeignKey(d => d.DepotId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
