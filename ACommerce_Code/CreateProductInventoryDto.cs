namespace ACommerce.Catalog.Products.DTOs.ProductInventory;

public class CreateProductInventoryDto
{
	public Guid ProductId { get; set; }
	public decimal QuantityInStock { get; set; }
	public decimal? LowStockThreshold { get; set; }
	public bool TrackInventory { get; set; } = true;
	public bool AllowBackorder { get; set; }
	public string? Warehouse { get; set; }
	public string? ShelfNumber { get; set; }
	public string? Location { get; set; }
	public Dictionary<string, string>? Metadata { get; set; }
}

