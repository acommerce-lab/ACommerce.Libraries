using ACommerce.Catalog.Attributes.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ACommerce.Catalog.Attributes.Configurations;

public class AttributeValueConfiguration : IEntityTypeConfiguration<AttributeValue>
{
	public void Configure(EntityTypeBuilder<AttributeValue> builder)
	{
		builder.ToTable("AttributeValues");

		builder.HasKey(e => e.Id);

		// منع EF Core من توليد ID تلقائياً - استخدم ID المحدد
		builder.Property(e => e.Id).ValueGeneratedNever();

		builder.Property(e => e.Value)
			.IsRequired()
			.HasMaxLength(500);

		builder.Property(e => e.DisplayName)
			.HasMaxLength(200);

		builder.Property(e => e.Code)
			.HasMaxLength(100);

		builder.Property(e => e.Description)
			.HasMaxLength(1000);

		builder.Property(e => e.ColorHex)
			.HasMaxLength(7);

		builder.Property(e => e.ImageUrl)
			.HasMaxLength(500);

		// Indexes
		builder.HasIndex(e => e.AttributeDefinitionId);
		builder.HasIndex(e => e.Code);
		builder.HasIndex(e => e.IsDeleted);

		// Relationships
		builder.HasOne(e => e.AttributeDefinition)
			.WithMany(d => d.Values)
			.HasForeignKey(e => e.AttributeDefinitionId)
			.OnDelete(DeleteBehavior.Restrict);

		builder.HasMany(e => e.ParentRelationships)
			.WithOne(r => r.ChildValue)
			.HasForeignKey(r => r.ChildValueId)
			.OnDelete(DeleteBehavior.Restrict);

		builder.HasMany(e => e.ChildRelationships)
			.WithOne(r => r.ParentValue)
			.HasForeignKey(r => r.ParentValueId)
			.OnDelete(DeleteBehavior.Restrict);
	}
}
