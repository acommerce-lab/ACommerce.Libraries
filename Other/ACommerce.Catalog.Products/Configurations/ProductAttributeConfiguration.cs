using ACommerce.Catalog.Products.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ACommerce.Catalog.Products.Configurations;

public class ProductAttributeConfiguration : IEntityTypeConfiguration<ProductAttribute>
{
	public void Configure(EntityTypeBuilder<ProductAttribute> builder)
	{
		builder.ToTable("ProductAttributes");

		builder.HasKey(e => e.Id);

		builder.Property(e => e.CustomValue)
			.HasMaxLength(1000);

		// Indexes
		builder.HasIndex(e => e.ProductId);
		builder.HasIndex(e => e.AttributeDefinitionId);
		builder.HasIndex(e => e.AttributeValueId);
		builder.HasIndex(e => new { e.ProductId, e.AttributeDefinitionId });
		builder.HasIndex(e => e.IsDeleted);

		// Relationships
		builder.HasOne(e => e.Product)
			.WithMany(p => p.Attributes)
			.HasForeignKey(e => e.ProductId)
			.OnDelete(DeleteBehavior.Cascade);

		// References to independent Attributes system
		builder.HasOne(e => e.AttributeDefinition)
			.WithMany()
			.HasForeignKey(e => e.AttributeDefinitionId)
			.OnDelete(DeleteBehavior.Restrict);

		builder.HasOne(e => e.AttributeValue)
			.WithMany()
			.HasForeignKey(e => e.AttributeValueId)
			.OnDelete(DeleteBehavior.Restrict);
	}
}
