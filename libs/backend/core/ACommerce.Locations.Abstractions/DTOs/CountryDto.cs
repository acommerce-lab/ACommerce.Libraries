namespace ACommerce.Locations.Abstractions.DTOs;

// ════════════════════════════════════════════════════════════
// Country DTOs
// ════════════════════════════════════════════════════════════

public class CountryResponseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? NameEn { get; set; }
    public string Code { get; set; } = string.Empty;
    public string? Code3 { get; set; }
    public string? PhoneCode { get; set; }
    public string? CurrencyCode { get; set; }
    public string? CurrencySymbol { get; set; }
    public string? Flag { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public bool IsActive { get; set; }
    public int SortOrder { get; set; }
}

public class CountryDetailDto : CountryResponseDto
{
    public int? NumericCode { get; set; }
    public string? CurrencyName { get; set; }
    public string? Timezone { get; set; }
    public int RegionsCount { get; set; }
    public int CitiesCount { get; set; }
}

public class CreateCountryDto
{
    public required string Name { get; set; }
    public string? NameEn { get; set; }
    public required string Code { get; set; }
    public string? Code3 { get; set; }
    public int? NumericCode { get; set; }
    public string? PhoneCode { get; set; }
    public string? CurrencyCode { get; set; }
    public string? CurrencyName { get; set; }
    public string? CurrencySymbol { get; set; }
    public string? Flag { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? Timezone { get; set; }
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; }
}

public class UpdateCountryDto
{
    public string? Name { get; set; }
    public string? NameEn { get; set; }
    public string? Code { get; set; }
    public string? Code3 { get; set; }
    public int? NumericCode { get; set; }
    public string? PhoneCode { get; set; }
    public string? CurrencyCode { get; set; }
    public string? CurrencyName { get; set; }
    public string? CurrencySymbol { get; set; }
    public string? Flag { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? Timezone { get; set; }
    public bool? IsActive { get; set; }
    public int? SortOrder { get; set; }
}
