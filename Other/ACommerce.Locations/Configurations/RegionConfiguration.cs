using ACommerce.Locations.Abstractions.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ACommerce.Locations.Configurations;

public class RegionConfiguration : IEntityTypeConfiguration<Region>
{
    public void Configure(EntityTypeBuilder<Region> builder)
    {
        builder.ToTable("Regions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.NameEn)
            .HasMaxLength(100);

        builder.Property(x => x.Code)
            .HasMaxLength(10);

        builder.Property(x => x.Timezone)
            .HasMaxLength(50);

        // Indexes
        builder.HasIndex(x => x.CountryId);
        builder.HasIndex(x => x.Code);
        builder.HasIndex(x => x.IsActive);
        builder.HasIndex(x => x.IsDeleted);
        builder.HasIndex(x => new { x.CountryId, x.Code });

        // Relations
        builder.HasMany(x => x.Cities)
            .WithOne(x => x.Region)
            .HasForeignKey(x => x.RegionId)
            .OnDelete(DeleteBehavior.Restrict);

        // Query Filter
        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
