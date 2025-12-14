namespace ACommerce.Catalog.Products.DTOs.ProductBrand;

public class CreateProductBrandDto
{
	public required string Name { get; set; }
	public required string Slug { get; set; }
	public string? Description { get; set; }
	public string? Logo { get; set; }
	public string? Website { get; set; }
	public string? Country { get; set; }
	public int SortOrder { get; set; }
	public Dictionary<string, string>? Metadata { get; set; }
}

