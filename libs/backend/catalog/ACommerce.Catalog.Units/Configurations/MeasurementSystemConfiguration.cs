using ACommerce.Catalog.Units.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ACommerce.Catalog.Units.Configurations;

public class MeasurementSystemConfiguration : IEntityTypeConfiguration<MeasurementSystem>
{
	public void Configure(EntityTypeBuilder<MeasurementSystem> builder)
	{
		builder.ToTable("MeasurementSystems");

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
			.WithOne(u => u.MeasurementSystem)
			.HasForeignKey(u => u.MeasurementSystemId)
			.OnDelete(DeleteBehavior.Restrict);
	}
}
