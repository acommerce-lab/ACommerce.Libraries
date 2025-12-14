using ACommerce.Catalog.Attributes.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ACommerce.Catalog.Attributes.Configurations;

public class AttributeValueRelationshipConfiguration : IEntityTypeConfiguration<AttributeValueRelationship>
{
	public void Configure(EntityTypeBuilder<AttributeValueRelationship> builder)
	{
		builder.ToTable("AttributeValueRelationships");

		builder.HasKey(e => e.Id);

		builder.Property(e => e.Type)
			.IsRequired()
			.HasMaxLength(50);

		// Indexes
		builder.HasIndex(e => e.ParentValueId);
		builder.HasIndex(e => e.ChildValueId);
		builder.HasIndex(e => new { e.ParentValueId, e.ChildValueId }).IsUnique();

		// Relationships
		builder.HasOne(e => e.ParentValue)
			.WithMany(v => v.ChildRelationships)
			.HasForeignKey(e => e.ParentValueId)
			.OnDelete(DeleteBehavior.Restrict);

		builder.HasOne(e => e.ChildValue)
			.WithMany(v => v.ParentRelationships)
			.HasForeignKey(e => e.ChildValueId)
			.OnDelete(DeleteBehavior.Restrict);
	}
}
