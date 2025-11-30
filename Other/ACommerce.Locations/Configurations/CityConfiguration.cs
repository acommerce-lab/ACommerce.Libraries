using ACommerce.Locations.Abstractions.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ACommerce.Locations.Configurations;

public class CityConfiguration : IEntityTypeConfiguration<City>
{
    public void Configure(EntityTypeBuilder<City> builder)
    {
        builder.ToTable("Cities");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.NameEn)
            .HasMaxLength(100);

        builder.Property(x => x.Code)
            .HasMaxLength(10);

        builder.Property(x => x.PostalCode)
            .HasMaxLength(20);

        builder.Property(x => x.Timezone)
            .HasMaxLength(50);

        // Indexes
        builder.HasIndex(x => x.RegionId);
        builder.HasIndex(x => x.Code);
        builder.HasIndex(x => x.IsActive);
        builder.HasIndex(x => x.IsCapital);
        builder.HasIndex(x => x.IsDeleted);
        builder.HasIndex(x => new { x.RegionId, x.Code });

        // Spatial Index (if using PostGIS)
        // builder.HasIndex(x => new { x.Latitude, x.Longitude });

        // Relations
        builder.HasMany(x => x.Neighborhoods)
            .WithOne(x => x.City)
            .HasForeignKey(x => x.CityId)
            .OnDelete(DeleteBehavior.Restrict);

        // Query Filter
        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
