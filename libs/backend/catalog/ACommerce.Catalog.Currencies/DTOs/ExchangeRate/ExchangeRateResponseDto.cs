namespace ACommerce.Catalog.Currencies.DTOs.ExchangeRate;

public class ExchangeRateResponseDto
{
	public Guid Id { get; set; }
	public Guid FromCurrencyId { get; set; }
	public string FromCurrencyCode { get; set; } = string.Empty;
	public string FromCurrencyName { get; set; } = string.Empty;
	public string FromCurrencySymbol { get; set; } = string.Empty;
	public Guid ToCurrencyId { get; set; }
	public string ToCurrencyCode { get; set; } = string.Empty;
	public string ToCurrencyName { get; set; } = string.Empty;
	public string ToCurrencySymbol { get; set; } = string.Empty;
	public decimal Rate { get; set; }
	public DateTime EffectiveFrom { get; set; }
	public DateTime? EffectiveTo { get; set; }
	public string Source { get; set; } = string.Empty;
	public bool IsActive { get; set; }
	public Dictionary<string, string> Metadata { get; set; } = new();
	public DateTime CreatedAt { get; set; }
	public DateTime? UpdatedAt { get; set; }
    public decimal InverseRate { get; set; }
    public string? RateType { get; set; }
}

