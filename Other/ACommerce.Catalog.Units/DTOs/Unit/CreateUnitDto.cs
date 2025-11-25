namespace ACommerce.Catalog.Units.DTOs.Unit;

public class CreateUnitDto
{
	public required string Name { get; set; }
	public required string Symbol { get; set; }
	public required string Code { get; set; }
	public Guid UnitCategoryId { get; set; }
	public Guid MeasurementSystemId { get; set; }
	public decimal ConversionToBase { get; set; }
	public string? ConversionFormula { get; set; }
	public int DecimalPlaces { get; set; } = 2;
	public bool IsStandard { get; set; } = true;
	public bool IsActive { get; set; } = true;
	public string? Description { get; set; }
	public int SortOrder { get; set; }
	public Dictionary<string, string>? Metadata { get; set; }
}

