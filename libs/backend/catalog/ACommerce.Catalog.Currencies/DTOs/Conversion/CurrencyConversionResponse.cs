namespace ACommerce.Catalog.Currencies.DTOs;

public class CurrencyConversionResponse
{
	public decimal OriginalAmount { get; set; }
	public string FromCurrencyCode { get; set; } = string.Empty;
	public string FromCurrencySymbol { get; set; } = string.Empty;
	public string FormattedOriginalAmount { get; set; } = string.Empty;

	public decimal ConvertedAmount { get; set; }
	public string ToCurrencyCode { get; set; } = string.Empty;
	public string ToCurrencySymbol { get; set; } = string.Empty;
	public string FormattedConvertedAmount { get; set; } = string.Empty;

	public decimal ExchangeRate { get; set; }
	public DateTime RateDate { get; set; }
	public string? RateSource { get; set; }
}

