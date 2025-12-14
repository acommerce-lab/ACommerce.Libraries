namespace ACommerce.Catalog.Currencies.DTOs.ExchangeRate;

public class CreateExchangeRateDto
{
	public Guid FromCurrencyId { get; set; }
	public Guid ToCurrencyId { get; set; }
	public decimal Rate { get; set; }
	public DateTime EffectiveFrom { get; set; }
	public DateTime? EffectiveTo { get; set; }
	public string Source { get; set; } = "Manual";
	public bool IsActive { get; set; } = true;
	public Dictionary<string, string>? Metadata { get; set; }
    public string? RateType { get; set; }
    public int? Priority { get; set; }
}