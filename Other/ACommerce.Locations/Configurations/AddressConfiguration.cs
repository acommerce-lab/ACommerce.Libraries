using ACommerce.Locations.Abstractions.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ACommerce.Locations.Configurations;

public class AddressConfiguration : IEntityTypeConfiguration<Address>
{
    public void Configure(EntityTypeBuilder<Address> builder)
    {
        builder.ToTable("Addresses");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.EntityType)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.Label)
            .HasMaxLength(50);

        builder.Property(x => x.Street)
            .HasMaxLength(200);

        builder.Property(x => x.BuildingNumber)
            .HasMaxLength(20);

        builder.Property(x => x.UnitNumber)
            .HasMaxLength(20);

        builder.Property(x => x.Floor)
            .HasMaxLength(10);

        builder.Property(x => x.PostalCode)
            .HasMaxLength(20);

        builder.Property(x => x.NationalAddressCode)
            .HasMaxLength(50);

        builder.Property(x => x.AdditionalDetails)
            .HasMaxLength(500);

        builder.Property(x => x.RecipientName)
            .HasMaxLength(100);

        builder.Property(x => x.RecipientPhone)
            .HasMaxLength(20);

        // Indexes
        builder.HasIndex(x => new { x.EntityType, x.EntityId });
        builder.HasIndex(x => x.CountryId);
        builder.HasIndex(x => x.CityId);
        builder.HasIndex(x => x.NeighborhoodId);
        builder.HasIndex(x => x.IsDefault);
        builder.HasIndex(x => x.IsDeleted);
        builder.HasIndex(x => x.PostalCode);
        builder.HasIndex(x => x.NationalAddressCode);

        // Relations
        builder.HasOne(x => x.Country)
            .WithMany()
            .HasForeignKey(x => x.CountryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Region)
            .WithMany()
            .HasForeignKey(x => x.RegionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.City)
            .WithMany()
            .HasForeignKey(x => x.CityId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Neighborhood)
            .WithMany()
            .HasForeignKey(x => x.NeighborhoodId)
            .OnDelete(DeleteBehavior.Restrict);

        // Query Filter
        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
