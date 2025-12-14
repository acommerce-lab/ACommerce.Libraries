namespace ACommerce.Catalog.Products.DTOs.ProductBrand;

public class ProductBrandResponseDto
{
	public Guid Id { get; set; }
	public string Name { get; set; } = string.Empty;
	public string Slug { get; set; } = string.Empty;
	public string? Description { get; set; }
	public string? Logo { get; set; }
	public string? Website { get; set; }
	public string? Country { get; set; }
	public int ProductsCount { get; set; }
	public int SortOrder { get; set; }
	public bool IsActive { get; set; }
	public Dictionary<string, string> Metadata { get; set; } = new();
	public DateTime CreatedAt { get; set; }
	public DateTime? UpdatedAt { get; set; }
}

