using ACommerce.Locations.Abstractions.Contracts;
using ACommerce.Locations.Abstractions.DTOs;
using ACommerce.Locations.Abstractions.Entities;
using Microsoft.EntityFrameworkCore;

namespace ACommerce.Locations.Services;

public class GeoService : IGeoService
{
    private readonly DbContext _context;
    private readonly ILocationService _locationService;

    public GeoService(DbContext context, ILocationService locationService)
    {
        _context = context;
        _locationService = locationService;
    }

    public async Task<List<GeoSearchResult<NeighborhoodResponseDto>>> FindNearbyNeighborhoodsAsync(
        GeoSearchRequest request,
        CancellationToken ct = default)
    {
        // Get all neighborhoods with coordinates
        var neighborhoods = await _context.Set<Neighborhood>()
            .AsNoTracking()
            .Include(x => x.City)
            .ThenInclude(c => c!.Region)
            .ThenInclude(r => r!.Country)
            .Where(x => x.IsActive && x.Latitude.HasValue && x.Longitude.HasValue)
            .ToListAsync(ct);

        var results = new List<GeoSearchResult<NeighborhoodResponseDto>>();

        foreach (var n in neighborhoods)
        {
            var distance = CalculateDistance(
                request.Latitude, request.Longitude,
                n.Latitude!.Value, n.Longitude!.Value);

            if (distance <= request.RadiusKm)
            {
                results.Add(new GeoSearchResult<NeighborhoodResponseDto>
                {
                    Item = new NeighborhoodResponseDto
                    {
                        Id = n.Id,
                        Name = n.Name,
                        NameEn = n.NameEn,
                        Code = n.Code,
                        CityId = n.CityId,
                        CityName = n.City?.Name,
                        RegionId = n.City?.RegionId,
                        RegionName = n.City?.Region?.Name,
                        CountryId = n.City?.Region?.CountryId,
                        CountryName = n.City?.Region?.Country?.Name,
                        Latitude = n.Latitude,
                        Longitude = n.Longitude,
                        PostalCode = n.PostalCode,
                        IsActive = n.IsActive,
                        SortOrder = n.SortOrder
                    },
                    DistanceKm = distance
                });
            }
        }

        return results
            .OrderBy(x => x.DistanceKm)
            .Take(request.Limit)
            .ToList();
    }

    public async Task<List<GeoSearchResult<CityResponseDto>>> FindNearbyCitiesAsync(
        GeoSearchRequest request,
        CancellationToken ct = default)
    {
        var cities = await _context.Set<City>()
            .AsNoTracking()
            .Include(x => x.Region)
            .ThenInclude(r => r!.Country)
            .Where(x => x.IsActive && x.Latitude.HasValue && x.Longitude.HasValue)
            .ToListAsync(ct);

        var results = new List<GeoSearchResult<CityResponseDto>>();

        foreach (var c in cities)
        {
            var distance = CalculateDistance(
                request.Latitude, request.Longitude,
                c.Latitude!.Value, c.Longitude!.Value);

            if (distance <= request.RadiusKm)
            {
                results.Add(new GeoSearchResult<CityResponseDto>
                {
                    Item = new CityResponseDto
                    {
                        Id = c.Id,
                        Name = c.Name,
                        NameEn = c.NameEn,
                        Code = c.Code,
                        RegionId = c.RegionId,
                        RegionName = c.Region?.Name,
                        CountryId = c.Region?.CountryId,
                        CountryName = c.Region?.Country?.Name,
                        Latitude = c.Latitude,
                        Longitude = c.Longitude,
                        IsActive = c.IsActive,
                        IsCapital = c.IsCapital,
                        SortOrder = c.SortOrder
                    },
                    DistanceKm = distance
                });
            }
        }

        return results
            .OrderBy(x => x.DistanceKm)
            .Take(request.Limit)
            .ToList();
    }

    public async Task<LocationHierarchyDto?> GetLocationFromCoordinatesAsync(
        double latitude,
        double longitude,
        CancellationToken ct = default)
    {
        // Find nearest neighborhood
        var nearestNeighborhood = await _context.Set<Neighborhood>()
            .AsNoTracking()
            .Include(x => x.City)
            .ThenInclude(c => c!.Region)
            .ThenInclude(r => r!.Country)
            .Where(x => x.IsActive && x.Latitude.HasValue && x.Longitude.HasValue)
            .ToListAsync(ct);

        Neighborhood? closest = null;
        double minDistance = double.MaxValue;

        foreach (var n in nearestNeighborhood)
        {
            var distance = CalculateDistance(
                latitude, longitude,
                n.Latitude!.Value, n.Longitude!.Value);

            if (distance < minDistance)
            {
                minDistance = distance;
                closest = n;
            }
        }

        if (closest == null) return null;

        return await _locationService.GetLocationHierarchyAsync(
            neighborhoodId: closest.Id,
            ct: ct);
    }

    public async Task<bool> IsPointInNeighborhoodAsync(
        double latitude,
        double longitude,
        Guid neighborhoodId,
        CancellationToken ct = default)
    {
        var neighborhood = await _context.Set<Neighborhood>()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == neighborhoodId, ct);

        if (neighborhood == null) return false;

        // If we have boundaries (GeoJSON), we could do proper polygon check
        // For now, just check if point is within ~1km of neighborhood center
        if (neighborhood.Latitude.HasValue && neighborhood.Longitude.HasValue)
        {
            var distance = CalculateDistance(
                latitude, longitude,
                neighborhood.Latitude.Value, neighborhood.Longitude.Value);

            return distance <= 2; // Within 2km of center
        }

        return false;
    }

    /// <summary>
    /// حساب المسافة بين نقطتين باستخدام صيغة Haversine
    /// </summary>
    public double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
    {
        const double R = 6371; // نصف قطر الأرض بالكيلومتر

        var dLat = ToRadians(lat2 - lat1);
        var dLon = ToRadians(lon2 - lon1);

        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return R * c;
    }

    private static double ToRadians(double degrees) => degrees * Math.PI / 180;
}
