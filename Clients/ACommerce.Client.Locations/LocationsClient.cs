using ACommerce.Client.Core.Http;

namespace ACommerce.Client.Locations;

/// <summary>
/// Client للتعامل مع المواقع الجغرافية
/// </summary>
public sealed class LocationsClient
{
    private readonly IApiClient _httpClient;
    private const string ServiceName = "Marketplace";
    private const string BasePath = "/api/locations";

    public LocationsClient(IApiClient httpClient)
    {
        _httpClient = httpClient;
    }

    // ══════════════════════════════════════════════════════════════════
    // Countries
    // ══════════════════════════════════════════════════════════════════

    /// <summary>
    /// الحصول على جميع الدول
    /// </summary>
    public async Task<List<CountryDto>?> GetCountriesAsync(
        bool activeOnly = true,
        CancellationToken cancellationToken = default)
    {
        return await _httpClient.GetAsync<List<CountryDto>>(
            ServiceName,
            $"{BasePath}/countries?activeOnly={activeOnly}",
            cancellationToken);
    }

    /// <summary>
    /// الحصول على دولة بالمعرف
    /// </summary>
    public async Task<CountryDetailDto?> GetCountryByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _httpClient.GetAsync<CountryDetailDto>(
            ServiceName,
            $"{BasePath}/countries/{id}",
            cancellationToken);
    }

    /// <summary>
    /// الحصول على دولة بالرمز (SA, AE, EG)
    /// </summary>
    public async Task<CountryDto?> GetCountryByCodeAsync(
        string code,
        CancellationToken cancellationToken = default)
    {
        return await _httpClient.GetAsync<CountryDto>(
            ServiceName,
            $"{BasePath}/countries/by-code/{code}",
            cancellationToken);
    }

    // ══════════════════════════════════════════════════════════════════
    // Regions
    // ══════════════════════════════════════════════════════════════════

    /// <summary>
    /// الحصول على مناطق دولة
    /// </summary>
    public async Task<List<RegionDto>?> GetRegionsByCountryAsync(
        Guid countryId,
        bool activeOnly = true,
        CancellationToken cancellationToken = default)
    {
        return await _httpClient.GetAsync<List<RegionDto>>(
            ServiceName,
            $"{BasePath}/countries/{countryId}/regions?activeOnly={activeOnly}",
            cancellationToken);
    }

    /// <summary>
    /// الحصول على منطقة بالمعرف
    /// </summary>
    public async Task<RegionDetailDto?> GetRegionByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _httpClient.GetAsync<RegionDetailDto>(
            ServiceName,
            $"{BasePath}/regions/{id}",
            cancellationToken);
    }

    // ══════════════════════════════════════════════════════════════════
    // Cities
    // ══════════════════════════════════════════════════════════════════

    /// <summary>
    /// الحصول على مدن منطقة
    /// </summary>
    public async Task<List<CityDto>?> GetCitiesByRegionAsync(
        Guid regionId,
        bool activeOnly = true,
        CancellationToken cancellationToken = default)
    {
        return await _httpClient.GetAsync<List<CityDto>>(
            ServiceName,
            $"{BasePath}/regions/{regionId}/cities?activeOnly={activeOnly}",
            cancellationToken);
    }

    /// <summary>
    /// الحصول على جميع مدن دولة
    /// </summary>
    public async Task<List<CityDto>?> GetCitiesByCountryAsync(
        Guid countryId,
        bool activeOnly = true,
        CancellationToken cancellationToken = default)
    {
        return await _httpClient.GetAsync<List<CityDto>>(
            ServiceName,
            $"{BasePath}/countries/{countryId}/cities?activeOnly={activeOnly}",
            cancellationToken);
    }

    /// <summary>
    /// الحصول على مدينة بالمعرف
    /// </summary>
    public async Task<CityDetailDto?> GetCityByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _httpClient.GetAsync<CityDetailDto>(
            ServiceName,
            $"{BasePath}/cities/{id}",
            cancellationToken);
    }

    // ══════════════════════════════════════════════════════════════════
    // Neighborhoods
    // ══════════════════════════════════════════════════════════════════

    /// <summary>
    /// الحصول على أحياء مدينة
    /// </summary>
    public async Task<List<NeighborhoodDto>?> GetNeighborhoodsByCityAsync(
        Guid cityId,
        bool activeOnly = true,
        CancellationToken cancellationToken = default)
    {
        return await _httpClient.GetAsync<List<NeighborhoodDto>>(
            ServiceName,
            $"{BasePath}/cities/{cityId}/neighborhoods?activeOnly={activeOnly}",
            cancellationToken);
    }

    /// <summary>
    /// الحصول على حي بالمعرف
    /// </summary>
    public async Task<NeighborhoodDetailDto?> GetNeighborhoodByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _httpClient.GetAsync<NeighborhoodDetailDto>(
            ServiceName,
            $"{BasePath}/neighborhoods/{id}",
            cancellationToken);
    }

    // ══════════════════════════════════════════════════════════════════
    // Search & Geo
    // ══════════════════════════════════════════════════════════════════

    /// <summary>
    /// البحث في المواقع
    /// </summary>
    public async Task<List<LocationSearchResult>?> SearchAsync(
        string query,
        Guid? countryId = null,
        int limit = 20,
        CancellationToken cancellationToken = default)
    {
        var url = $"{BasePath}/search?q={Uri.EscapeDataString(query)}&limit={limit}";
        if (countryId.HasValue)
            url += $"&countryId={countryId}";

        return await _httpClient.GetAsync<List<LocationSearchResult>>(
            ServiceName,
            url,
            cancellationToken);
    }

    /// <summary>
    /// الحصول على التسلسل الهرمي لموقع
    /// </summary>
    public async Task<LocationHierarchyDto?> GetHierarchyAsync(
        Guid? neighborhoodId = null,
        Guid? cityId = null,
        Guid? regionId = null,
        Guid? countryId = null,
        CancellationToken cancellationToken = default)
    {
        var queryParams = new List<string>();
        if (neighborhoodId.HasValue) queryParams.Add($"neighborhoodId={neighborhoodId}");
        if (cityId.HasValue) queryParams.Add($"cityId={cityId}");
        if (regionId.HasValue) queryParams.Add($"regionId={regionId}");
        if (countryId.HasValue) queryParams.Add($"countryId={countryId}");

        var url = $"{BasePath}/hierarchy";
        if (queryParams.Count > 0)
            url += "?" + string.Join("&", queryParams);

        return await _httpClient.GetAsync<LocationHierarchyDto>(
            ServiceName,
            url,
            cancellationToken);
    }

    /// <summary>
    /// البحث عن المدن القريبة
    /// GET /api/locations/nearby/cities
    /// </summary>
    public async Task<List<GeoSearchResult<CityDto>>?> FindNearbyCitiesAsync(
        double latitude,
        double longitude,
        double radiusKm = 50,
        int limit = 20,
        CancellationToken cancellationToken = default)
    {
        return await _httpClient.GetAsync<List<GeoSearchResult<CityDto>>>(
            ServiceName,
            $"{BasePath}/nearby/cities?lat={latitude}&lng={longitude}&radiusKm={radiusKm}&limit={limit}",
            cancellationToken);
    }

    /// <summary>
    /// البحث عن الأحياء القريبة
    /// GET /api/locations/nearby/neighborhoods
    /// </summary>
    public async Task<List<GeoSearchResult<NeighborhoodDto>>?> FindNearbyNeighborhoodsAsync(
        double latitude,
        double longitude,
        double radiusKm = 10,
        int limit = 50,
        CancellationToken cancellationToken = default)
    {
        return await _httpClient.GetAsync<List<GeoSearchResult<NeighborhoodDto>>>(
            ServiceName,
            $"{BasePath}/nearby/neighborhoods?lat={latitude}&lng={longitude}&radiusKm={radiusKm}&limit={limit}",
            cancellationToken);
    }

    /// <summary>
    /// تحديد الموقع العكسي من الإحداثيات
    /// GET /api/locations/reverse-geocode
    /// </summary>
    public async Task<LocationHierarchyDto?> ReverseGeocodeAsync(
        double latitude,
        double longitude,
        CancellationToken cancellationToken = default)
    {
        return await _httpClient.GetAsync<LocationHierarchyDto>(
            ServiceName,
            $"{BasePath}/reverse-geocode?lat={latitude}&lon={longitude}",
            cancellationToken);
    }
}

// ══════════════════════════════════════════════════════════════════
// DTOs
// ══════════════════════════════════════════════════════════════════

/// <summary>
/// بيانات الدولة
/// </summary>
public class CountryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? NameEn { get; set; }
    public string Code { get; set; } = string.Empty;
    public string? PhoneCode { get; set; }
    public string? CurrencyCode { get; set; }
    public string? Flag { get; set; }
    public bool IsActive { get; set; }
}

/// <summary>
/// تفاصيل الدولة
/// </summary>
public sealed class CountryDetailDto : CountryDto
{
    public string? Code3 { get; set; }
    public int? NumericCode { get; set; }
    public string? CurrencyName { get; set; }
    public string? CurrencySymbol { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? Timezone { get; set; }
    public int RegionsCount { get; set; }
    public int CitiesCount { get; set; }
}

/// <summary>
/// بيانات المنطقة
/// </summary>
public class RegionDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? NameEn { get; set; }
    public string? Code { get; set; }
    public RegionType Type { get; set; }
    public Guid CountryId { get; set; }
    public bool IsActive { get; set; }
}

/// <summary>
/// تفاصيل المنطقة
/// </summary>
public sealed class RegionDetailDto : RegionDto
{
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? CountryName { get; set; }
    public int CitiesCount { get; set; }
}

/// <summary>
/// نوع المنطقة
/// </summary>
public enum RegionType
{
    Region = 1,
    Emirate = 2,
    Governorate = 3,
    State = 4,
    Province = 5
}

/// <summary>
/// بيانات المدينة
/// </summary>
public class CityDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? NameEn { get; set; }
    public string? Code { get; set; }
    public bool IsCapital { get; set; }
    public Guid RegionId { get; set; }
    public bool IsActive { get; set; }
}

/// <summary>
/// تفاصيل المدينة
/// </summary>
public sealed class CityDetailDto : CityDto
{
    public int? Population { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? RegionName { get; set; }
    public string? CountryName { get; set; }
    public int NeighborhoodsCount { get; set; }
}

/// <summary>
/// بيانات الحي
/// </summary>
public class NeighborhoodDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? NameEn { get; set; }
    public string? Code { get; set; }
    public string? PostalCode { get; set; }
    public Guid CityId { get; set; }
    public bool IsActive { get; set; }
}

/// <summary>
/// تفاصيل الحي
/// </summary>
public sealed class NeighborhoodDetailDto : NeighborhoodDto
{
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? CityName { get; set; }
    public string? RegionName { get; set; }
    public string? CountryName { get; set; }
}

/// <summary>
/// نتيجة البحث في المواقع
/// </summary>
public sealed class LocationSearchResult
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

/// <summary>
/// التسلسل الهرمي للموقع
/// </summary>
public sealed class LocationHierarchyDto
{
    public CountryDto? Country { get; set; }
    public RegionDto? Region { get; set; }
    public CityDto? City { get; set; }
    public NeighborhoodDto? Neighborhood { get; set; }
}

/// <summary>
/// نتيجة البحث الجغرافي
/// </summary>
public sealed class GeoSearchResult<T>
{
    public T Item { get; set; } = default!;
    public double DistanceKm { get; set; }
}
