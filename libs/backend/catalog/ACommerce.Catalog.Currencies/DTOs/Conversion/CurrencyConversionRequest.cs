namespace ACommerce.Catalog.Currencies.DTOs;

public class CurrencyConversionRequest
{
	public decimal Amount { get; set; }
	public Guid FromCurrencyId { get; set; }
	public Guid ToCurrencyId { get; set; }
	public DateTime? Date { get; set; }
	public string? RateType { get; set; }
}

