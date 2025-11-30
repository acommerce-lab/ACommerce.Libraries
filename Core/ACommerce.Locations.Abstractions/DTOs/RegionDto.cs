using ACommerce.Locations.Abstractions.Entities;

namespace ACommerce.Locations.Abstractions.DTOs;

// ════════════════════════════════════════════════════════════
// Region DTOs
// ════════════════════════════════════════════════════════════

public class RegionResponseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? NameEn { get; set; }
    public string? Code { get; set; }
    public RegionType Type { get; set; }
    public Guid CountryId { get; set; }
    public string? CountryName { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public bool IsActive { get; set; }
    public int SortOrder { get; set; }
}

public class RegionDetailDto : RegionResponseDto
{
    public string? Timezone { get; set; }
    public int CitiesCount { get; set; }
    public CountryResponseDto? Country { get; set; }
}

public class CreateRegionDto
{
    public required string Name { get; set; }
    public string? NameEn { get; set; }
    public string? Code { get; set; }
    public RegionType Type { get; set; } = RegionType.Region;
    public required Guid CountryId { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? Timezone { get; set; }
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; }
}

public class UpdateRegionDto
{
    public string? Name { get; set; }
    public string? NameEn { get; set; }
    public string? Code { get; set; }
    public RegionType? Type { get; set; }
    public Guid? CountryId { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? Timezone { get; set; }
    public bool? IsActive { get; set; }
    public int? SortOrder { get; set; }
}
