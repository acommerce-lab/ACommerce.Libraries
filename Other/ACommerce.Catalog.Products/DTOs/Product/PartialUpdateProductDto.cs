using ACommerce.Catalog.Products.Enums;

namespace ACommerce.Catalog.Products.DTOs.Product;

public class PartialUpdateProductDto
{
	public string? Name { get; set; }
	public ProductType? Type { get; set; }
	public ProductStatus? Status { get; set; }
	public string? ShortDescription { get; set; }
	public string? LongDescription { get; set; }
	public string? Barcode { get; set; }
	public decimal? Weight { get; set; }
	public Guid? WeightUnitId { get; set; }
	public decimal? Length { get; set; }
	public decimal? Width { get; set; }
	public decimal? Height { get; set; }
	public Guid? DimensionUnitId { get; set; }
	public List<string>? Images { get; set; }
	public string? FeaturedImage { get; set; }
	public List<string>? Tags { get; set; }
	public bool? IsFeatured { get; set; }
	public bool? IsNew { get; set; }
	public DateTime? NewUntil { get; set; }
	public int? SortOrder { get; set; }
	public Dictionary<string, string>? Metadata { get; set; }
}

