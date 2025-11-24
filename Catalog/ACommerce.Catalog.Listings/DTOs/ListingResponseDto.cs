using ACommerce.Catalog.Listings.Enums;

namespace ACommerce.Catalog.Listings.DTOs;

public class ListingResponseDto
{
	public Guid Id { get; set; }
	public Guid VendorId { get; set; }
	public Guid ProductId { get; set; }
	public string? VendorSku { get; set; }
	public ListingStatus Status { get; set; }
	public decimal Price { get; set; }
	public decimal? CompareAtPrice { get; set; }
	public int QuantityAvailable { get; set; }
	public int QuantityReserved { get; set; }
	public bool IsActive { get; set; }
	public int TotalSales { get; set; }
	public decimal? Rating { get; set; }
	public DateTime CreatedAt { get; set; }
}
