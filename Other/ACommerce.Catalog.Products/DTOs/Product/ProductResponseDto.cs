using ACommerce.Catalog.Products.Enums;

namespace ACommerce.Catalog.Products.DTOs.Product;

public class ProductResponseDto
{
	public Guid Id { get; set; }
	public string Name { get; set; } = string.Empty;
	public string Sku { get; set; } = string.Empty;
	public ProductType Type { get; set; }
	public ProductStatus Status { get; set; }
	public string? ShortDescription { get; set; }
	public string? LongDescription { get; set; }
	public string? Barcode { get; set; }
	public decimal? Weight { get; set; }
	public Guid? WeightUnitId { get; set; }
	public string? WeightUnitName { get; set; }
	public string? WeightUnitSymbol { get; set; }
	public decimal? Length { get; set; }
	public decimal? Width { get; set; }
	public decimal? Height { get; set; }
	public Guid? DimensionUnitId { get; set; }
	public string? DimensionUnitName { get; set; }
	public string? DimensionUnitSymbol { get; set; }
	public List<string> Images { get; set; } = new();
	public string? FeaturedImage { get; set; }
	public List<string> Tags { get; set; } = new();
	public bool IsFeatured { get; set; }
	public bool IsNew { get; set; }
	public DateTime? NewUntil { get; set; }
	public Guid? ParentProductId { get; set; }
	public int VariantsCount { get; set; }
	public int CategoriesCount { get; set; }
	public int AttributesCount { get; set; }
	public Dictionary<string, string> Metadata { get; set; } = new();
	public DateTime CreatedAt { get; set; }
	public DateTime? UpdatedAt { get; set; }
}

