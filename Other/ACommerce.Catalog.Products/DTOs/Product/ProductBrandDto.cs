namespace ACommerce.Catalog.Products.DTOs.Product;

public class ProductBrandDto
{
	public Guid Id { get; set; }
	public string Name { get; set; } = string.Empty;
	public string Slug { get; set; } = string.Empty;
	public string? Logo { get; set; }
}

