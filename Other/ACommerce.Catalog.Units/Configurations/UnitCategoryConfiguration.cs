using ACommerce.Catalog.Units.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ACommerce.Catalog.Units.Configurations;

public class UnitCategoryConfiguration : IEntityTypeConfiguration<UnitCategory>
{
	public void Configure(EntityTypeBuilder<UnitCategory> builder)
	{
		builder.ToTable("UnitCategories");

		builder.HasKey(e => e.Id);

		builder.Property(e => e.Name)
			.IsRequired()
			.HasMaxLength(200);

		builder.Property(e => e.Code)
			.IsRequired()
			.HasMaxLength(100);

		builder.Property(e => e.Description)
			.HasMaxLength(1000);

		// Indexes
		builder.HasIndex(e => e.Code).IsUnique();
		builder.HasIndex(e => e.Name);
		builder.HasIndex(e => e.IsDeleted);

		// Relationships
		builder.HasMany(e => e.Units)
			.WithOne(u => u.UnitCategory)
			.HasForeignKey(u => u.UnitCategoryId)
			.OnDelete(DeleteBehavior.Restrict);

		// BaseUnit relationship - optional self-reference to a Unit
		builder.HasOne(e => e.BaseUnit)
			.WithMany()
			.HasForeignKey(e => e.BaseUnitId)
			.OnDelete(DeleteBehavior.Restrict);
	}
}
