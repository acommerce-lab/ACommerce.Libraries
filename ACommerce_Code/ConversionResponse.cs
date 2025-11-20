namespace ACommerce.Catalog.Units.DTOs;

/// <summary>
/// ????? ???????
/// </summary>
public class ConversionResponse
{
	public decimal OriginalValue { get; set; }
	public string FromUnit { get; set; } = string.Empty;
	public string FromSymbol { get; set; } = string.Empty;
	public decimal ConvertedValue { get; set; }
	public string ToUnit { get; set; } = string.Empty;
	public string ToSymbol { get; set; } = string.Empty;
	public decimal ConversionFactor { get; set; }
	public string Formula { get; set; } = string.Empty;
}

