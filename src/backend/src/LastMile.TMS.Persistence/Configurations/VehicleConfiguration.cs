using LastMile.TMS.Domain.Entities;
using LastMile.TMS.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LastMile.TMS.Persistence.Configurations;

public class VehicleConfiguration : IEntityTypeConfiguration<Vehicle>
{
    public void Configure(EntityTypeBuilder<Vehicle> builder)
    {
        // Registration plate
        builder.Property(v => v.RegistrationPlate)
            .HasMaxLength(20)
            .IsRequired();
        builder.HasIndex(v => v.RegistrationPlate).IsUnique();

        builder.Property(v => v.Type).HasConversion<string>().HasMaxLength(20);
        builder.Property(v => v.Status).HasConversion<string>().HasMaxLength(30);

        // Capacity
        builder.Property(v => v.ParcelCapacity).HasDefaultValue(0);
        builder.Property(v => v.WeightCapacityKg).HasPrecision(10, 2);

        // Indexes
        builder.HasIndex(v => v.Status);

        // Relationships
        builder.HasOne(v => v.Depot)
            .WithMany()
            .HasForeignKey(v => v.DepotId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
