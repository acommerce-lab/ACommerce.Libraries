namespace ACommerce.Locations.Abstractions.DTOs;

// ════════════════════════════════════════════════════════════
// City DTOs
// ════════════════════════════════════════════════════════════

public class CityResponseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? NameEn { get; set; }
    public string? Code { get; set; }
    public Guid RegionId { get; set; }
    public string? RegionName { get; set; }
    public Guid? CountryId { get; set; }
    public string? CountryName { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public bool IsActive { get; set; }
    public bool IsCapital { get; set; }
    public int SortOrder { get; set; }
}

public class CityDetailDto : CityResponseDto
{
    public string? PostalCode { get; set; }
    public int? Population { get; set; }
    public string? Timezone { get; set; }
    public int NeighborhoodsCount { get; set; }
    public RegionResponseDto? Region { get; set; }
}

public class CreateCityDto
{
    public required string Name { get; set; }
    public string? NameEn { get; set; }
    public string? Code { get; set; }
    public required Guid RegionId { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? PostalCode { get; set; }
    public int? Population { get; set; }
    public string? Timezone { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsCapital { get; set; }
    public int SortOrder { get; set; }
}

public class UpdateCityDto
{
    public string? Name { get; set; }
    public string? NameEn { get; set; }
    public string? Code { get; set; }
    public Guid? RegionId { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? PostalCode { get; set; }
    public int? Population { get; set; }
    public string? Timezone { get; set; }
    public bool? IsActive { get; set; }
    public bool? IsCapital { get; set; }
    public int? SortOrder { get; set; }
}
