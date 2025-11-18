namespace ACommerce.Catalog.Currencies.DTOs.Currency;

public class CurrencyResponseDto
{
	public Guid Id { get; set; }
	public string Name { get; set; } = string.Empty;
	public string CurrencyCode { get; set; } = string.Empty;
	public string Symbol { get; set; } = string.Empty;
	public int DecimalPlaces { get; set; }
	public bool SymbolBeforeAmount { get; set; }
	public string ThousandsSeparator { get; set; } = string.Empty;
	public string DecimalSeparator { get; set; } = string.Empty;
	public bool IsBaseCurrency { get; set; }
	public bool IsActive { get; set; }
	public List<string> Countries { get; set; } = new();
	public Dictionary<string, string> Metadata { get; set; } = new();
	public DateTime CreatedAt { get; set; }
	public DateTime? UpdatedAt { get; set; }
}

