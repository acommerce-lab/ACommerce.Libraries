namespace ACommerce.Catalog.Products.DTOs.Product;

public class ProductAttributeDto
{
	public Guid Id { get; set; }
	public Guid AttributeDefinitionId { get; set; }
	public string AttributeName { get; set; } = string.Empty;
	public string AttributeCode { get; set; } = string.Empty;
	public Guid? AttributeValueId { get; set; }
	public string ValueName { get; set; } = string.Empty;
	public string? CustomValue { get; set; }
	public bool IsVariant { get; set; }
}

