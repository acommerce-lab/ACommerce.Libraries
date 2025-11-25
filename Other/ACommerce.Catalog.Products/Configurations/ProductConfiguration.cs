using ACommerce.Catalog.Products.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ACommerce.Catalog.Products.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
	public void Configure(EntityTypeBuilder<Product> builder)
	{
		builder.ToTable("Products");

		builder.HasKey(e => e.Id);

		builder.Property(e => e.Name)
			.IsRequired()
			.HasMaxLength(500);

		builder.Property(e => e.Sku)
			.IsRequired()
			.HasMaxLength(100);

		builder.Property(e => e.Type)
			.IsRequired()
			.HasConversion<string>();

		builder.Property(e => e.Status)
			.IsRequired()
			.HasConversion<string>();

		builder.Property(e => e.ShortDescription)
			.HasMaxLength(1000);

		builder.Property(e => e.Barcode)
			.HasMaxLength(100);

		builder.Property(e => e.Weight)
			.HasPrecision(18, 4);

		builder.Property(e => e.Length)
			.HasPrecision(18, 4);

		builder.Property(e => e.Width)
			.HasPrecision(18, 4);

		builder.Property(e => e.Height)
			.HasPrecision(18, 4);

		builder.Property(e => e.FeaturedImage)
			.HasMaxLength(500);

		// Indexes
		builder.HasIndex(e => e.Sku).IsUnique();
		builder.HasIndex(e => e.Name);
		builder.HasIndex(e => e.Type);
		builder.HasIndex(e => e.Status);
		builder.HasIndex(e => e.IsDeleted);
		builder.HasIndex(e => e.IsFeatured);

		// Relationships

		// Self-reference for variants
		builder.HasOne(e => e.ParentProduct)
			.WithMany(p => p.Variants)
			.HasForeignKey(e => e.ParentProductId)
			.OnDelete(DeleteBehavior.Restrict);

		// Units relationships (independent system)
		builder.HasOne(e => e.WeightUnit)
			.WithMany()
			.HasForeignKey(e => e.WeightUnitId)
			.OnDelete(DeleteBehavior.Restrict);

		builder.HasOne(e => e.DimensionUnit)
			.WithMany()
			.HasForeignKey(e => e.DimensionUnitId)
			.OnDelete(DeleteBehavior.Restrict);

		// Other relationships
		builder.HasMany(e => e.Attributes)
			.WithOne(a => a.Product)
			.HasForeignKey(a => a.ProductId)
			.OnDelete(DeleteBehavior.Cascade);

		builder.HasMany(e => e.Prices)
			.WithOne(p => p.Product)
			.HasForeignKey(p => p.ProductId)
			.OnDelete(DeleteBehavior.Cascade);

		builder.HasOne(e => e.Inventory)
			.WithOne(i => i.Product)
			.HasForeignKey<ProductInventory>(i => i.ProductId)
			.OnDelete(DeleteBehavior.Cascade);

		builder.HasMany(e => e.RelatedProducts)
			.WithOne(r => r.SourceProduct)
			.HasForeignKey(r => r.SourceProductId)
			.OnDelete(DeleteBehavior.Cascade);

		builder.HasMany(e => e.Reviews)
			.WithOne(r => r.Product)
			.HasForeignKey(r => r.ProductId)
			.OnDelete(DeleteBehavior.Cascade);
	}
}
