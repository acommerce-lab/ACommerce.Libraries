namespace ACommerce.Catalog.Currencies.DTOs.Currency;

public class CreateCurrencyDto
{
	public required string Name { get; set; }
	public required string CurrencyCode { get; set; }
	public required string Symbol { get; set; }
	public int DecimalPlaces { get; set; } = 2;
	public bool SymbolBeforeAmount { get; set; } = true;
	public string ThousandsSeparator { get; set; } = ",";
	public string DecimalSeparator { get; set; } = ".";
	public bool IsBaseCurrency { get; set; }
	public List<string>? Countries { get; set; }
	public Dictionary<string, string>? Metadata { get; set; }
}

