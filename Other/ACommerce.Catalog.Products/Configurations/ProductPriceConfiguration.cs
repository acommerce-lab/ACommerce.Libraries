using ACommerce.Catalog.Products.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ACommerce.Catalog.Products.Configurations;

public class ProductPriceConfiguration : IEntityTypeConfiguration<ProductPrice>
{
	public void Configure(EntityTypeBuilder<ProductPrice> builder)
	{
		builder.ToTable("ProductPrices");

		builder.HasKey(e => e.Id);

		builder.Property(e => e.BasePrice)
			.HasPrecision(18, 4);

		builder.Property(e => e.SalePrice)
			.HasPrecision(18, 4);

		builder.Property(e => e.DiscountPercentage)
			.HasPrecision(5, 2);

		builder.Property(e => e.Market)
			.HasMaxLength(10);

		builder.Property(e => e.CustomerSegment)
			.HasMaxLength(50);

		// Indexes
		builder.HasIndex(e => e.ProductId);
		builder.HasIndex(e => e.CurrencyId);
		builder.HasIndex(e => new { e.ProductId, e.CurrencyId, e.Market, e.CustomerSegment });
		builder.HasIndex(e => e.IsActive);
		builder.HasIndex(e => e.IsDeleted);

		// Ignore computed property
		builder.Ignore(e => e.EffectivePrice);

		// Relationships
		builder.HasOne(e => e.Product)
			.WithMany(p => p.Prices)
			.HasForeignKey(e => e.ProductId)
			.OnDelete(DeleteBehavior.Cascade);

		// Reference to independent Currency system
		builder.HasOne(e => e.Currency)
			.WithMany()
			.HasForeignKey(e => e.CurrencyId)
			.OnDelete(DeleteBehavior.Restrict);
	}
}
