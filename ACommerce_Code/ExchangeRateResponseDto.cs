namespace ACommerce.Catalog.Currencies.DTOs.ExchangeRate;

public class ExchangeRateResponseDto
{
	public Guid Id { get; set; }
	public Guid FromCurrencyId { get; set; }
	public string FromCurrencyCode { get; set; } = string.Empty;
	public string FromCurrencyName { get; set; } = string.Empty;
	public Guid ToCurrencyId { get; set; }
	public string ToCurrencyCode { get; set; } = string.Empty;
	public string ToCurrencyName { get; set; } = string.Empty;
	public decimal Rate { get; set; }
	public decimal InverseRate { get; set; }
	public DateTime EffectiveDate { get; set; }
	public DateTime? ExpiryDate { get; set; }
	public string? Source { get; set; }
	public int Priority { get; set; }
	public string RateType { get; set; } = string.Empty;
	public bool IsFixed { get; set; }
	public DateTime CreatedAt { get; set; }
}

