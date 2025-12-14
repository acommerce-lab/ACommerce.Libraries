using ACommerce.Catalog.Attributes.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ACommerce.Catalog.Attributes.Configurations;

public class CategoryAttributeMappingConfiguration : IEntityTypeConfiguration<CategoryAttributeMapping>
{
	public void Configure(EntityTypeBuilder<CategoryAttributeMapping> builder)
	{
		builder.ToTable("CategoryAttributeMappings");

		builder.HasKey(e => e.Id);

		// Indexes
		builder.HasIndex(e => e.CategoryId);
		builder.HasIndex(e => e.AttributeDefinitionId);
		builder.HasIndex(e => e.IsDeleted);

		// فهرس مركب للبحث السريع عن خصائص فئة معينة
		builder.HasIndex(e => new { e.CategoryId, e.SortOrder });

		// ضمان عدم تكرار نفس الخاصية في نفس الفئة
		builder.HasIndex(e => new { e.CategoryId, e.AttributeDefinitionId }).IsUnique();

		// Relationships
		builder.HasOne(e => e.AttributeDefinition)
			.WithMany()
			.HasForeignKey(e => e.AttributeDefinitionId)
			.OnDelete(DeleteBehavior.Restrict);
	}
}
