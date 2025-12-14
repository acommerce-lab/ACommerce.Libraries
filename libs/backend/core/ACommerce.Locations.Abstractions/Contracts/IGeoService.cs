using ACommerce.Locations.Abstractions.DTOs;

namespace ACommerce.Locations.Abstractions.Contracts;

/// <summary>
/// خدمة البحث الجغرافي
/// </summary>
public interface IGeoService
{
    /// <summary>
    /// البحث عن أحياء قريبة من نقطة
    /// </summary>
    Task<List<GeoSearchResult<NeighborhoodResponseDto>>> FindNearbyNeighborhoodsAsync(
        GeoSearchRequest request,
        CancellationToken ct = default);

    /// <summary>
    /// البحث عن مدن قريبة من نقطة
    /// </summary>
    Task<List<GeoSearchResult<CityResponseDto>>> FindNearbyCitiesAsync(
        GeoSearchRequest request,
        CancellationToken ct = default);

    /// <summary>
    /// تحديد الموقع من الإحداثيات (Reverse Geocoding)
    /// </summary>
    Task<LocationHierarchyDto?> GetLocationFromCoordinatesAsync(
        double latitude,
        double longitude,
        CancellationToken ct = default);

    /// <summary>
    /// التحقق من وقوع نقطة داخل حدود حي
    /// </summary>
    Task<bool> IsPointInNeighborhoodAsync(
        double latitude,
        double longitude,
        Guid neighborhoodId,
        CancellationToken ct = default);

    /// <summary>
    /// حساب المسافة بين نقطتين
    /// </summary>
    double CalculateDistance(
        double lat1, double lon1,
        double lat2, double lon2);
}
