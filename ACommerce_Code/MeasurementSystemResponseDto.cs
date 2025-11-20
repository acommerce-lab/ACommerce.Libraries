namespace ACommerce.Catalog.Units.DTOs.MeasurementSystem;

public class MeasurementSystemResponseDto
{
	public Guid Id { get; set; }
	public string Name { get; set; } = string.Empty;
	public string Code { get; set; } = string.Empty;
	public string? Description { get; set; }
	public bool IsDefault { get; set; }
	public List<string> Countries { get; set; } = new();
	public Dictionary<string, string> Metadata { get; set; } = new();
	public int UnitsCount { get; set; }
	public DateTime CreatedAt { get; set; }
	public DateTime? UpdatedAt { get; set; }
}

