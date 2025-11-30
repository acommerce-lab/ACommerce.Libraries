namespace ACommerce.Locations.Abstractions.DTOs;

// ════════════════════════════════════════════════════════════
// Neighborhood DTOs
// ════════════════════════════════════════════════════════════

public class NeighborhoodResponseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? NameEn { get; set; }
    public string? Code { get; set; }
    public Guid CityId { get; set; }
    public string? CityName { get; set; }
    public Guid? RegionId { get; set; }
    public string? RegionName { get; set; }
    public Guid? CountryId { get; set; }
    public string? CountryName { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? PostalCode { get; set; }
    public bool IsActive { get; set; }
    public int SortOrder { get; set; }
}

public class NeighborhoodDetailDto : NeighborhoodResponseDto
{
    public string? Boundaries { get; set; }
    public CityResponseDto? City { get; set; }
}

public class CreateNeighborhoodDto
{
    public required string Name { get; set; }
    public string? NameEn { get; set; }
    public string? Code { get; set; }
    public required Guid CityId { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? PostalCode { get; set; }
    public string? Boundaries { get; set; }
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; }
}

public class UpdateNeighborhoodDto
{
    public string? Name { get; set; }
    public string? NameEn { get; set; }
    public string? Code { get; set; }
    public Guid? CityId { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? PostalCode { get; set; }
    public string? Boundaries { get; set; }
    public bool? IsActive { get; set; }
    public int? SortOrder { get; set; }
}
