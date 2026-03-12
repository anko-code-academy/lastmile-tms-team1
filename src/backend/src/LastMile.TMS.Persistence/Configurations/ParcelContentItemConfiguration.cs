using LastMile.TMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LastMile.TMS.Persistence.Configurations;

public class ParcelContentItemConfiguration : IEntityTypeConfiguration<ParcelContentItem>
{
    public void Configure(EntityTypeBuilder<ParcelContentItem> builder)
    {
        builder.Property(pci => pci.HsCode).HasMaxLength(10);
        builder.Property(pci => pci.Description).HasMaxLength(500);
        builder.Property(pci => pci.Quantity).HasDefaultValue(1);
        builder.Property(pci => pci.UnitValue).HasPrecision(18, 2);
        builder.Property(pci => pci.Currency).HasMaxLength(3).IsRequired();
        builder.Property(pci => pci.Weight).HasPrecision(10, 3);
        builder.Property(pci => pci.WeightUnit).HasConversion<string>().HasMaxLength(2);
        builder.Property(pci => pci.CountryOfOrigin).HasMaxLength(2).IsRequired();

        builder.HasIndex(pci => pci.HsCode);

        builder.HasOne(pci => pci.Parcel)
            .WithMany(p => p.ContentItems)
            .HasForeignKey(pci => pci.ParcelId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}