using ACommerce.Catalog.Currencies.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ACommerce.Catalog.Currencies.Configurations;

public class CurrencyConfiguration : IEntityTypeConfiguration<Currency>
{
	public void Configure(EntityTypeBuilder<Currency> builder)
	{
		builder.ToTable("Currencies");

		builder.HasKey(e => e.Id);

		builder.Property(e => e.Code)
			.IsRequired()
			.HasMaxLength(3); // ISO 4217

		builder.Property(e => e.Name)
			.IsRequired()
			.HasMaxLength(200);

		builder.Property(e => e.Symbol)
			.IsRequired()
			.HasMaxLength(10);

		builder.Property(e => e.ThousandsSeparator)
			.HasMaxLength(5);

		builder.Property(e => e.DecimalSeparator)
			.HasMaxLength(5);

		// Indexes
		builder.HasIndex(e => e.Code).IsUnique();
		builder.HasIndex(e => e.Name);
		builder.HasIndex(e => e.IsBaseCurrency);
		builder.HasIndex(e => e.IsDeleted);

		// Relationships
		builder.HasMany(e => e.ExchangeRatesFrom)
			.WithOne(r => r.FromCurrency)
			.HasForeignKey(r => r.FromCurrencyId)
			.OnDelete(DeleteBehavior.Restrict);

		builder.HasMany(e => e.ExchangeRatesTo)
			.WithOne(r => r.ToCurrency)
			.HasForeignKey(r => r.ToCurrencyId)
			.OnDelete(DeleteBehavior.Restrict);
	}
}
