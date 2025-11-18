namespace ACommerce.Catalog.Units.DTOs.Unit;

public class UnitResponseDto
{
	public Guid Id { get; set; }
	public string Name { get; set; } = string.Empty;
	public string Symbol { get; set; } = string.Empty;
	public string Code { get; set; } = string.Empty;
	public Guid MeasurementCategoryId { get; set; }
	public string MeasurementCategoryName { get; set; } = string.Empty;
	public Guid MeasurementSystemId { get; set; }
	public string MeasurementSystemName { get; set; } = string.Empty;
	public decimal ConversionToBase { get; set; }
	public string? ConversionFormula { get; set; }
	public int DecimalPlaces { get; set; }
	public bool IsStandard { get; set; }
	public string? Description { get; set; }
	public int SortOrder { get; set; }
	public bool IsActive { get; set; }
	public Dictionary<string, string> Metadata { get; set; } = new();
	public DateTime CreatedAt { get; set; }
	public DateTime? UpdatedAt { get; set; }
}

