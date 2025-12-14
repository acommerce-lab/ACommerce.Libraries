namespace ACommerce.Catalog.Attributes.DTOs.AttributeDefinition;

public class PartialUpdateAttributeDefinitionDto
{
	public string? Name { get; set; }
	public string? Description { get; set; }
	public bool? IsRequired { get; set; }
	public bool? IsFilterable { get; set; }
	public bool? IsVisibleInList { get; set; }
	public bool? IsVisibleInDetail { get; set; }
	public int? SortOrder { get; set; }
	public string? ValidationRules { get; set; }
	public string? DefaultValue { get; set; }
	public string? Unit { get; set; }
	public Dictionary<string, string>? Metadata { get; set; }
}

