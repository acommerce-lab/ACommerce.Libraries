namespace ACommerce.Catalog.Products.DTOs.ProductPrice;

public class CreateProductPriceDto
{
	public Guid ProductId { get; set; }
	public Guid CurrencyId { get; set; }
	public decimal BasePrice { get; set; }
	public decimal? SalePrice { get; set; }
	public decimal? DiscountPercentage { get; set; }
	public DateTime? SaleStartDate { get; set; }
	public DateTime? SaleEndDate { get; set; }
	public string? Market { get; set; }
	public string? CustomerSegment { get; set; }
	public int MinQuantity { get; set; } = 1;
	public int? MaxQuantity { get; set; }
	public Dictionary<string, string>? Metadata { get; set; }
}

