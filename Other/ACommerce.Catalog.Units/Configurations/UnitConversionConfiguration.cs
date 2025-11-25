using ACommerce.Catalog.Units.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ACommerce.Catalog.Units.Configurations;

public class UnitConversionConfiguration : IEntityTypeConfiguration<UnitConversion>
{
	public void Configure(EntityTypeBuilder<UnitConversion> builder)
	{
		builder.ToTable("UnitConversions");

		builder.HasKey(e => e.Id);

		builder.Property(e => e.ConversionFactor)
			.HasPrecision(18, 6);

		builder.Property(e => e.Formula)
			.HasMaxLength(500);

		// Indexes
		builder.HasIndex(e => e.FromUnitId);
		builder.HasIndex(e => e.ToUnitId);
		builder.HasIndex(e => new { e.FromUnitId, e.ToUnitId }).IsUnique();
		builder.HasIndex(e => e.IsDeleted);

		// Relationships
		builder.HasOne(e => e.FromUnit)
			.WithMany(u => u.ConversionsFrom)
			.HasForeignKey(e => e.FromUnitId)
			.OnDelete(DeleteBehavior.Restrict);

		builder.HasOne(e => e.ToUnit)
			.WithMany(u => u.ConversionsTo)
			.HasForeignKey(e => e.ToUnitId)
			.OnDelete(DeleteBehavior.Restrict);
	}
}
