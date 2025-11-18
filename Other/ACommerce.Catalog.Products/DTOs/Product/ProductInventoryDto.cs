using ACommerce.Catalog.Products.Enums;

namespace ACommerce.Catalog.Products.DTOs.Product;

public class ProductInventoryDto
{
	public decimal QuantityInStock { get; set; }
	public decimal AvailableQuantity { get; set; }
	public StockStatus Status { get; set; }
	public bool TrackInventory { get; set; }
	public bool AllowBackorder { get; set; }
}

