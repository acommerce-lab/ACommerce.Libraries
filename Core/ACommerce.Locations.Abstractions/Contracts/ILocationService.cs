using ACommerce.Locations.Abstractions.DTOs;
using ACommerce.Locations.Abstractions.Entities;

namespace ACommerce.Locations.Abstractions.Contracts;

/// <summary>
/// خدمة إدارة المواقع الجغرافية
/// </summary>
public interface ILocationService
{
    // ══════════════════════════════════════════════════════════════════
    // Countries
    // ══════════════════════════════════════════════════════════════════

    Task<List<CountryResponseDto>> GetCountriesAsync(
        bool activeOnly = true,
        CancellationToken ct = default);

    Task<CountryDetailDto?> GetCountryByIdAsync(Guid id, CancellationToken ct = default);

    Task<CountryResponseDto?> GetCountryByCodeAsync(string code, CancellationToken ct = default);

    // ══════════════════════════════════════════════════════════════════
    // Regions
    // ══════════════════════════════════════════════════════════════════

    Task<List<RegionResponseDto>> GetRegionsByCountryAsync(
        Guid countryId,
        bool activeOnly = true,
        CancellationToken ct = default);

    Task<RegionDetailDto?> GetRegionByIdAsync(Guid id, CancellationToken ct = default);

    // ══════════════════════════════════════════════════════════════════
    // Cities
    // ══════════════════════════════════════════════════════════════════

    Task<List<CityResponseDto>> GetCitiesByRegionAsync(
        Guid regionId,
        bool activeOnly = true,
        CancellationToken ct = default);

    Task<List<CityResponseDto>> GetCitiesByCountryAsync(
        Guid countryId,
        bool activeOnly = true,
        CancellationToken ct = default);

    Task<CityDetailDto?> GetCityByIdAsync(Guid id, CancellationToken ct = default);

    // ══════════════════════════════════════════════════════════════════
    // Neighborhoods
    // ══════════════════════════════════════════════════════════════════

    Task<List<NeighborhoodResponseDto>> GetNeighborhoodsByCityAsync(
        Guid cityId,
        bool activeOnly = true,
        CancellationToken ct = default);

    Task<NeighborhoodDetailDto?> GetNeighborhoodByIdAsync(Guid id, CancellationToken ct = default);

    // ══════════════════════════════════════════════════════════════════
    // Hierarchy & Search
    // ══════════════════════════════════════════════════════════════════

    /// <summary>
    /// الحصول على التسلسل الهرمي الكامل لموقع
    /// </summary>
    Task<LocationHierarchyDto?> GetLocationHierarchyAsync(
        Guid? neighborhoodId = null,
        Guid? cityId = null,
        Guid? regionId = null,
        Guid? countryId = null,
        CancellationToken ct = default);

    /// <summary>
    /// البحث في المواقع
    /// </summary>
    Task<List<LocationSearchResult>> SearchLocationsAsync(
        string query,
        Guid? countryId = null,
        int limit = 20,
        CancellationToken ct = default);
}

/// <summary>
/// نتيجة البحث في المواقع
/// </summary>
public class LocationSearchResult
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? NameEn { get; set; }
    public LocationLevel Level { get; set; }
    public string? ParentName { get; set; }
    public string FullPath { get; set; } = string.Empty;
}

/// <summary>
/// مستوى الموقع
/// </summary>
public enum LocationLevel
{
    Country = 1,
    Region = 2,
    City = 3,
    Neighborhood = 4
}
