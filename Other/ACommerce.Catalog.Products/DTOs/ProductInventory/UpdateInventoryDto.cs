namespace ACommerce.Catalog.Products.DTOs.ProductInventory;

public class UpdateInventoryDto
{
	public decimal? QuantityInStock { get; set; }
	public decimal? QuantityReserved { get; set; }
	public decimal? LowStockThreshold { get; set; }
	public bool? TrackInventory { get; set; }
	public bool? AllowBackorder { get; set; }
	public string? Warehouse { get; set; }
	public string? ShelfNumber { get; set; }
	public string? Location { get; set; }
}

