using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ACommerce.Locations.Abstractions.Contracts;
using ACommerce.Locations.Abstractions.DTOs;
using ACommerce.Locations.Dev.Services;
using ACommerce.Locations.Models;

namespace ACommerce.Locations.Dev
{
    /// <summary>
    /// Lightweight development adapter implementing ILocationService by delegating
    /// to the simple in-memory location provider. This is intended for local dev/demo only.
    /// </summary>
    public class DevLocationService : ILocationService
    {
        private readonly InMemoryLocationService _inner;

        public DevLocationService(InMemoryLocationService inner)
        {
            _inner = inner;
        }

        public Task<List<CountryResponseDto>> GetCountriesAsync(bool activeOnly = true, CancellationToken ct = default)
        {
            return Task.FromResult(new List<CountryResponseDto>());
        }

        public Task<CountryDetailDto?> GetCountryByIdAsync(Guid id, CancellationToken ct = default) => Task.FromResult<CountryDetailDto?>(null);
        public Task<CountryResponseDto?> GetCountryByCodeAsync(string code, CancellationToken ct = default) => Task.FromResult<CountryResponseDto?>(null);

        public Task<List<RegionResponseDto>> GetRegionsByCountryAsync(Guid countryId, bool activeOnly = true, CancellationToken ct = default)
            => Task.FromResult(new List<RegionResponseDto>());
        public Task<RegionDetailDto?> GetRegionByIdAsync(Guid id, CancellationToken ct = default) => Task.FromResult<RegionDetailDto?>(null);

        public Task<List<CityResponseDto>> GetCitiesByRegionAsync(Guid regionId, bool activeOnly = true, CancellationToken ct = default)
            => Task.FromResult(new List<CityResponseDto>());
        public Task<List<CityResponseDto>> GetCitiesByCountryAsync(Guid countryId, bool activeOnly = true, CancellationToken ct = default)
            => Task.FromResult(new List<CityResponseDto>());
        public Task<CityDetailDto?> GetCityByIdAsync(Guid id, CancellationToken ct = default) => Task.FromResult<CityDetailDto?>(null);

        public Task<List<NeighborhoodResponseDto>> GetNeighborhoodsByCityAsync(Guid cityId, bool activeOnly = true, CancellationToken ct = default)
            => Task.FromResult(new List<NeighborhoodResponseDto>());
        public Task<NeighborhoodDetailDto?> GetNeighborhoodByIdAsync(Guid id, CancellationToken ct = default) => Task.FromResult<NeighborhoodDetailDto?>(null);

        public async Task<LocationHierarchyDto?> GetLocationHierarchyAsync(Guid? neighborhoodId = null, Guid? cityId = null, Guid? regionId = null, Guid? countryId = null, CancellationToken ct = default)
        {
            // Very small demo implementation: if a cityId/neighborhoodId isn't provided return null.
            if (!neighborhoodId.HasValue && !cityId.HasValue && !regionId.HasValue && !countryId.HasValue)
                return null;

            // return a stub hierarchy using the first in-memory item when available
            var all = (await _inner.AutocompleteAsync(string.Empty)).ToList();
            var first = all.FirstOrDefault();
            if (first == null) return null;

            var city = new CityResponseDto { Id = Guid.NewGuid(), Name = first.DisplayName, Latitude = first.Point.Latitude, Longitude = first.Point.Longitude };
            return new LocationHierarchyDto { City = city };
        }

        public async Task<List<LocationSearchResult>> SearchLocationsAsync(string query, Guid? countryId = null, int limit = 20, CancellationToken ct = default)
        {
            var items = (await _inner.AutocompleteAsync(query, null, null, limit)).ToList();
            var results = items.Select(d => new LocationSearchResult
            {
                Id = Guid.NewGuid(),
                Name = d.DisplayName,
                NameEn = null,
                Level = d.Type == "city" ? LocationLevel.City : LocationLevel.Neighborhood,
                ParentName = null,
                FullPath = d.DisplayName
            }).Take(limit).ToList();

            return results;
        }
    }

    /// <summary>
    /// Lightweight geo service for development — delegates to InMemoryLocationService for reverse/nearby.
    /// </summary>
    public class DevGeoService : IGeoService
    {
        private readonly InMemoryLocationService _inner;

        public DevGeoService(InMemoryLocationService inner)
        {
            _inner = inner;
        }

        public async Task<List<GeoSearchResult<NeighborhoodResponseDto>>> FindNearbyNeighborhoodsAsync(GeoSearchRequest request, CancellationToken ct = default)
        {
            var all = (await _inner.NearbyAsync(request.Latitude, request.Longitude, (int)(request.RadiusKm * 1000))).ToList();
            return all.Select(d => new GeoSearchResult<NeighborhoodResponseDto>
            {
                DistanceKm = 0,
                Item = new NeighborhoodResponseDto { Id = Guid.NewGuid(), Name = d.DisplayName, Latitude = d.Point.Latitude, Longitude = d.Point.Longitude }
            }).Take(request.Limit).ToList();
        }

        public async Task<List<GeoSearchResult<CityResponseDto>>> FindNearbyCitiesAsync(GeoSearchRequest request, CancellationToken ct = default)
        {
            var all = (await _inner.NearbyAsync(request.Latitude, request.Longitude, (int)(request.RadiusKm * 1000))).ToList();
            return all.Select(d => new GeoSearchResult<CityResponseDto>
            {
                DistanceKm = 0,
                Item = new CityResponseDto { Id = Guid.NewGuid(), Name = d.DisplayName, Latitude = d.Point.Latitude, Longitude = d.Point.Longitude }
            }).Take(request.Limit).ToList();
        }

        public async Task<LocationHierarchyDto?> GetLocationFromCoordinatesAsync(double latitude, double longitude, CancellationToken ct = default)
        {
            var loc = await _inner.ReverseGeocodeAsync(latitude, longitude);
            if (loc == null) return null;

            var city = new CityResponseDto { Id = Guid.NewGuid(), Name = loc.DisplayName, Latitude = loc.Point.Latitude, Longitude = loc.Point.Longitude };
            return new LocationHierarchyDto { City = city };
        }

        public Task<bool> IsPointInNeighborhoodAsync(double latitude, double longitude, Guid neighborhoodId, CancellationToken ct = default)
        {
            return Task.FromResult(false);
        }

        public double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            var p1 = new GeoPoint { Latitude = lat1, Longitude = lon1 };
            var p2 = new GeoPoint { Latitude = lat2, Longitude = lon2 };
            return p1.DistanceTo(p2);
        }
    }
}
