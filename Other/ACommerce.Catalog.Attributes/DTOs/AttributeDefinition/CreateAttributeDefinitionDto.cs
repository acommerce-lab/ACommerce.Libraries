using ACommerce.Catalog.Attributes.Enums;

namespace ACommerce.Catalog.Attributes.DTOs.AttributeDefinition;

public class CreateAttributeDefinitionDto
{
	public required string Name { get; set; }
	public required string Code { get; set; }
	public AttributeType Type { get; set; }
	public string? Description { get; set; }
	public bool IsRequired { get; set; }
	public bool IsFilterable { get; set; }
	public bool IsVisibleInList { get; set; } = true;
	public bool IsVisibleInDetail { get; set; } = true;
	public int SortOrder { get; set; }
	public string? ValidationRules { get; set; }
	public string? DefaultValue { get; set; }
	public string? Unit { get; set; }
	public Dictionary<string, string>? Metadata { get; set; }
}

