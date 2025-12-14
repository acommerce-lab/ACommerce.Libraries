namespace ACommerce.Catalog.Products.DTOs.ProductCategory;

public class CreateProductCategoryDto
{
	public required string Name { get; set; }
	public required string Slug { get; set; }
	public string? Description { get; set; }
	public string? Image { get; set; }
	public string? Icon { get; set; }
	public Guid? ParentCategoryId { get; set; }
	public int SortOrder { get; set; }
	public Dictionary<string, string>? Metadata { get; set; }
}

