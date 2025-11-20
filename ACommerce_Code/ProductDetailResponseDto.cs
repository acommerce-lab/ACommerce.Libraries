namespace ACommerce.Catalog.Products.DTOs.Product;

/// <summary>
/// ?????? ?????? ??????? (?? ????????)
/// </summary>
public class ProductDetailResponseDto : ProductResponseDto
{
	public List<ProductCategoryDto> Categories { get; set; } = new();
	public List<ProductBrandDto> Brands { get; set; } = new();
	public List<ProductAttributeDto> Attributes { get; set; } = new();
	public List<ProductPriceDto> Prices { get; set; } = new();
	public ProductInventoryDto? Inventory { get; set; }
	public List<ProductResponseDto> Variants { get; set; } = new();
	public List<ProductResponseDto> RelatedProducts { get; set; } = new();
	public ProductReviewSummaryDto? ReviewSummary { get; set; }
}

