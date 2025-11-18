namespace ACommerce.Catalog.Products.DTOs.Product;

public class ProductAttributeDto
{
	public Guid Id { get; set; }
	public string AttributeName { get; set; } = string.Empty;
	public string? AttributeValue { get; set; }
	public string? CustomValue { get; set; }
	public bool IsVariant { get; set; }
}

