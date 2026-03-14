using System.Text.Json;
using LastMile.TMS.Domain.Common;
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

        builder.OwnsOne(d => d.OperatingHours, oh =>
       {
           oh.Property(p => p.Schedule)
               .HasConversion(
                   v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                   v => JsonSerializer.Deserialize<IReadOnlyCollection<DailyOperatingHours>>(
                       v,
                       (JsonSerializerOptions?)null
                   )
                   ?? new List<DailyOperatingHours>().AsReadOnly())
               .HasColumnName("OperatingHoursJson")
               .IsRequired();
       });

        builder.HasOne(d => d.Address)
           .WithMany()
           .HasForeignKey(d => d.AddressId)
           .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(d => d.Zones)
           .WithOne(z => z.Depot)
           .HasForeignKey(z => z.DepotId)
           .OnDelete(DeleteBehavior.Cascade);
    }
}
