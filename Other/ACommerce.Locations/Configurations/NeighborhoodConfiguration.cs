using ACommerce.Locations.Abstractions.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ACommerce.Locations.Configurations;

public class NeighborhoodConfiguration : IEntityTypeConfiguration<Neighborhood>
{
    public void Configure(EntityTypeBuilder<Neighborhood> builder)
    {
        builder.ToTable("Neighborhoods");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.NameEn)
            .HasMaxLength(100);

        builder.Property(x => x.Code)
            .HasMaxLength(20);

        builder.Property(x => x.PostalCode)
            .HasMaxLength(20);

        builder.Property(x => x.Boundaries)
            .HasColumnType("text"); // GeoJSON

        // Indexes
        builder.HasIndex(x => x.CityId);
        builder.HasIndex(x => x.Code);
        builder.HasIndex(x => x.PostalCode);
        builder.HasIndex(x => x.IsActive);
        builder.HasIndex(x => x.IsDeleted);
        builder.HasIndex(x => new { x.CityId, x.Code });

        // Query Filter
        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
