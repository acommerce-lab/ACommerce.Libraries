namespace ACommerce.Catalog.Attributes.DTOs.AttributeValue;

public class AttributeValueResponseDto
{
	public Guid Id { get; set; }
	public Guid AttributeDefinitionId { get; set; }
	public string AttributeDefinitionName { get; set; } = string.Empty;
	public string Value { get; set; } = string.Empty;
	public string? DisplayName { get; set; }
	public string? Code { get; set; }
	public string? Description { get; set; }
	public string? ColorHex { get; set; }
	public string? ImageUrl { get; set; }
	public int SortOrder { get; set; }
	public bool IsActive { get; set; }
	public Dictionary<string, string> Metadata { get; set; } = new();
	public DateTime CreatedAt { get; set; }
	public DateTime? UpdatedAt { get; set; }
}

