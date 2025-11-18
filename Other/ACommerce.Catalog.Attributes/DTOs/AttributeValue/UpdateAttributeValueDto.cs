namespace ACommerce.Catalog.Attributes.DTOs.AttributeValue;

public class UpdateAttributeValueDto
{
	public string? Value { get; set; }
	public string? DisplayName { get; set; }
	public string? Code { get; set; }
	public string? Description { get; set; }
	public string? ColorHex { get; set; }
	public string? ImageUrl { get; set; }
	public int? SortOrder { get; set; }
	public bool? IsActive { get; set; }
	public Dictionary<string, string>? Metadata { get; set; }
}

