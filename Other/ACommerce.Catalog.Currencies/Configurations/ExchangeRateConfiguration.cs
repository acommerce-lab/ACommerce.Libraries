using ACommerce.Catalog.Currencies.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ACommerce.Catalog.Currencies.Configurations;

public class ExchangeRateConfiguration : IEntityTypeConfiguration<ExchangeRate>
{
	public void Configure(EntityTypeBuilder<ExchangeRate> builder)
	{
		builder.ToTable("ExchangeRates");

		builder.HasKey(e => e.Id);

		builder.Property(e => e.Rate)
			.HasPrecision(18, 6);

		builder.Property(e => e.Source)
			.IsRequired()
			.HasMaxLength(100);

		// Indexes
		builder.HasIndex(e => e.FromCurrencyId);
		builder.HasIndex(e => e.ToCurrencyId);
		builder.HasIndex(e => new { e.FromCurrencyId, e.ToCurrencyId, e.EffectiveFrom });
		builder.HasIndex(e => e.IsActive);
		builder.HasIndex(e => e.IsDeleted);

		// Relationships
		builder.HasOne(e => e.FromCurrency)
			.WithMany(c => c.ExchangeRatesFrom)
			.HasForeignKey(e => e.FromCurrencyId)
			.OnDelete(DeleteBehavior.Restrict);

		builder.HasOne(e => e.ToCurrency)
			.WithMany(c => c.ExchangeRatesTo)
			.HasForeignKey(e => e.ToCurrencyId)
			.OnDelete(DeleteBehavior.Restrict);
	}
}
