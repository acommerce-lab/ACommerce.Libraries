using ACommerce.Locations.Abstractions.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ACommerce.Locations.Configurations;

public class CountryConfiguration : IEntityTypeConfiguration<Country>
{
    public void Configure(EntityTypeBuilder<Country> builder)
    {
        builder.ToTable("Countries");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.NameEn)
            .HasMaxLength(100);

        builder.Property(x => x.Code)
            .IsRequired()
            .HasMaxLength(2);

        builder.Property(x => x.Code3)
            .HasMaxLength(3);

        builder.Property(x => x.PhoneCode)
            .HasMaxLength(10);

        builder.Property(x => x.CurrencyCode)
            .HasMaxLength(3);

        builder.Property(x => x.CurrencyName)
            .HasMaxLength(50);

        builder.Property(x => x.CurrencySymbol)
            .HasMaxLength(10);

        builder.Property(x => x.Flag)
            .HasMaxLength(100);

        builder.Property(x => x.Timezone)
            .HasMaxLength(50);

        // Indexes
        builder.HasIndex(x => x.Code).IsUnique();
        builder.HasIndex(x => x.Code3);
        builder.HasIndex(x => x.IsActive);
        builder.HasIndex(x => x.IsDeleted);

        // Relations
        builder.HasMany(x => x.Regions)
            .WithOne(x => x.Country)
            .HasForeignKey(x => x.CountryId)
            .OnDelete(DeleteBehavior.Restrict);

        // Query Filter for Soft Delete
        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
