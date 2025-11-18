namespace ACommerce.Catalog.Units.DTOs;

/// <summary>
/// ??? ?????
/// </summary>
public class ConversionRequest
{
	public decimal Value { get; set; }
	public Guid FromUnitId { get; set; }
	public Guid ToUnitId { get; set; }
}

