namespace ACommerce.Catalog.Products.DTOs.Product;

public class ProductPriceDto
{
	public Guid Id { get; set; }
	public string CurrencyCode { get; set; } = string.Empty;
	public decimal BasePrice { get; set; }
	public decimal? SalePrice { get; set; }
	public decimal EffectivePrice { get; set; }
	public string FormattedPrice { get; set; } = string.Empty;
	public decimal? DiscountPercentage { get; set; }
	public string? Market { get; set; }
}

