using ACommerce.Catalog.Attributes.Enums;

namespace ACommerce.Catalog.Attributes.DTOs.AttributeDefinition;

public class AttributeDefinitionResponseDto
{
	public Guid Id { get; set; }
	public string Name { get; set; } = string.Empty;
	public string Code { get; set; } = string.Empty;
	public AttributeType Type { get; set; }
	public string? Description { get; set; }
	public bool IsRequired { get; set; }
	public bool IsFilterable { get; set; }
	public bool IsVisibleInList { get; set; }
	public bool IsVisibleInDetail { get; set; }
	public int SortOrder { get; set; }
	public string? ValidationRules { get; set; }
	public string? DefaultValue { get; set; }
	public string? Unit { get; set; }
	public Dictionary<string, string> Metadata { get; set; } = new();
	public int ValuesCount { get; set; }
	public DateTime CreatedAt { get; set; }
	public DateTime? UpdatedAt { get; set; }
}

