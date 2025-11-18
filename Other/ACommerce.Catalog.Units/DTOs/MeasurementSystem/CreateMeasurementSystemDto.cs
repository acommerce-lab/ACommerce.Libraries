namespace ACommerce.Catalog.Units.DTOs.MeasurementSystem;

public class CreateMeasurementSystemDto
{
	public required string Name { get; set; }
	public required string Code { get; set; }
	public string? Description { get; set; }
	public bool IsDefault { get; set; }
	public List<string>? Countries { get; set; }
	public Dictionary<string, string>? Metadata { get; set; }
}

