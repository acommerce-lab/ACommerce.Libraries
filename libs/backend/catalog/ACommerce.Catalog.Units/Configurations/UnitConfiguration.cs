using ACommerce.Catalog.Units.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ACommerce.Catalog.Units.Configurations;

public class UnitConfiguration : IEntityTypeConfiguration<Unit>
{
	public void Configure(EntityTypeBuilder<Unit> builder)
	{
		builder.ToTable("Units");

		builder.HasKey(e => e.Id);

		builder.Property(e => e.Name)
			.IsRequired()
			.HasMaxLength(200);

		builder.Property(e => e.Symbol)
			.IsRequired()
			.HasMaxLength(20);

		builder.Property(e => e.Description)
			.HasMaxLength(1000);

		builder.Property(e => e.ConversionToBase)
			.HasPrecision(18, 6);

		builder.Property(e => e.ConversionFormula)
			.HasMaxLength(500);

		// Indexes
		builder.HasIndex(e => e.Symbol);
		builder.HasIndex(e => e.UnitCategoryId);
		builder.HasIndex(e => e.MeasurementSystemId);
		builder.HasIndex(e => e.IsDeleted);

		// Relationships
		builder.HasOne(e => e.UnitCategory)
			.WithMany(c => c.Units)
			.HasForeignKey(e => e.UnitCategoryId)
			.OnDelete(DeleteBehavior.Restrict);

		builder.HasOne(e => e.MeasurementSystem)
			.WithMany(m => m.Units)
			.HasForeignKey(e => e.MeasurementSystemId)
			.OnDelete(DeleteBehavior.Restrict);

		builder.HasMany(e => e.ConversionsFrom)
			.WithOne(c => c.FromUnit)
			.HasForeignKey(c => c.FromUnitId)
			.OnDelete(DeleteBehavior.Restrict);

		builder.HasMany(e => e.ConversionsTo)
			.WithOne(c => c.ToUnit)
			.HasForeignKey(c => c.ToUnitId)
			.OnDelete(DeleteBehavior.Restrict);
	}
}
