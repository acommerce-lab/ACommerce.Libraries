namespace ACommerce.Catalog.Currencies.DTOs.ExchangeRate;

public class CreateExchangeRateDto
{
	public Guid FromCurrencyId { get; set; }
	public Guid ToCurrencyId { get; set; }
	public decimal Rate { get; set; }
	public DateTime? EffectiveDate { get; set; }
	public DateTime? ExpiryDate { get; set; }
	public string? Source { get; set; }
	public int Priority { get; set; } = 1;
	public string RateType { get; set; } = "official";
	public bool IsFixed { get; set; }
}

