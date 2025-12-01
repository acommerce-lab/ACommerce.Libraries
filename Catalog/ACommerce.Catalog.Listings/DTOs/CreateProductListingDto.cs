namespace ACommerce.Catalog.Listings.DTOs;

public class CreateProductListingDto
{
	public Guid VendorId { get; set; }
	public Guid ProductId { get; set; }
	public string? VendorSku { get; set; }
	public required decimal Price { get; set; }
	public decimal? CompareAtPrice { get; set; }
	public decimal? Cost { get; set; }
	public Guid? CurrencyId { get; set; }
	public int QuantityAvailable { get; set; }
	public int? ProcessingTime { get; set; }
	public string? VendorNotes { get; set; }
}
