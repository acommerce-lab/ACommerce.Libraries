using ACommerce.Locations.Abstractions.Contracts;
using ACommerce.Locations.Abstractions.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace ACommerce.Locations.Api.Controllers;

[ApiController]
[Route("api/locations")]
public class LocationSearchController : ControllerBase
{
    private readonly ILocationService _locationService;
    private readonly IGeoService _geoService;

    public LocationSearchController(
        ILocationService locationService,
        IGeoService geoService)
    {
        _locationService = locationService;
        _geoService = geoService;
    }

    /// <summary>
    /// البحث في المواقع
    /// </summary>
    [HttpGet("search")]
    public async Task<ActionResult<List<LocationSearchResult>>> Search(
        [FromQuery] string q,
        [FromQuery] Guid? countryId = null,
        [FromQuery] int limit = 20,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(q) || q.Length < 2)
            return BadRequest("Search query must be at least 2 characters");

        var results = await _locationService.SearchLocationsAsync(q, countryId, limit, ct);
        return Ok(results);
    }

    /// <summary>
    /// الحصول على التسلسل الهرمي للموقع
    /// </summary>
    [HttpGet("hierarchy")]
    public async Task<ActionResult<LocationHierarchyDto>> GetHierarchy(
        [FromQuery] Guid? neighborhoodId = null,
        [FromQuery] Guid? cityId = null,
        [FromQuery] Guid? regionId = null,
        [FromQuery] Guid? countryId = null,
        CancellationToken ct = default)
    {
        var hierarchy = await _locationService.GetLocationHierarchyAsync(
            neighborhoodId, cityId, regionId, countryId, ct);

        if (hierarchy == null) return NotFound();
        return Ok(hierarchy);
    }

    /// <summary>
    /// البحث عن أحياء قريبة
    /// </summary>
    [HttpGet("nearby/neighborhoods")]
    public async Task<ActionResult<List<GeoSearchResult<NeighborhoodResponseDto>>>> FindNearbyNeighborhoods(
        [FromQuery] double lat,
        [FromQuery] double lng,
        [FromQuery] double radiusKm = 10,
        [FromQuery] int limit = 50,
        CancellationToken ct = default)
    {
        var request = new GeoSearchRequest
        {
            Latitude = lat,
            Longitude = lng,
            RadiusKm = radiusKm,
            Limit = limit
        };

        var results = await _geoService.FindNearbyNeighborhoodsAsync(request, ct);
        return Ok(results);
    }

    /// <summary>
    /// البحث عن مدن قريبة
    /// </summary>
    [HttpGet("nearby/cities")]
    public async Task<ActionResult<List<GeoSearchResult<CityResponseDto>>>> FindNearbyCities(
        [FromQuery] double lat,
        [FromQuery] double lng,
        [FromQuery] double radiusKm = 50,
        [FromQuery] int limit = 20,
        CancellationToken ct = default)
    {
        var request = new GeoSearchRequest
        {
            Latitude = lat,
            Longitude = lng,
            RadiusKm = radiusKm,
            Limit = limit
        };

        var results = await _geoService.FindNearbyCitiesAsync(request, ct);
        return Ok(results);
    }

    /// <summary>
    /// تحديد الموقع من الإحداثيات
    /// </summary>
    [HttpGet("reverse-geocode")]
    public async Task<ActionResult<LocationHierarchyDto>> ReverseGeocode(
        [FromQuery] double lat,
        [FromQuery] double lng,
        CancellationToken ct = default)
    {
        var location = await _geoService.GetLocationFromCoordinatesAsync(lat, lng, ct);
        if (location == null) return NotFound();
        return Ok(location);
    }
}
