using ACommerce.Locations.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ACommerce.Locations.Dev.Services
{
    // Simple in-memory implementation for development and tests.
    public class InMemoryLocationService
    {
        private readonly List<LocationDto> _data = new()
        {
            new LocationDto { Id = "loc-1", DisplayName = "Riyadh, Saudi Arabia", Type = "city", Point = new PointDto(24.7136,46.6753), Confidence = 1.0 },
            new LocationDto { Id = "loc-2", DisplayName = "Jeddah, Saudi Arabia", Type = "city", Point = new PointDto(21.5433,39.1728), Confidence = 1.0 },
            new LocationDto { Id = "loc-3", DisplayName = "King Fahd Rd, Riyadh", Type = "street", Point = new PointDto(24.7139,46.6755), Confidence = 0.9 }
        };

        public Task<IEnumerable<LocationDto>> AutocompleteAsync(string query, double? lat = null, double? lon = null, int limit = 10)
        {
            var q = (query ?? string.Empty).ToLowerInvariant();
            var results = _data.Where(d => d.DisplayName.ToLowerInvariant().Contains(q)).Take(limit);
            return Task.FromResult(results);
        }

        public Task<IEnumerable<LocationDto>> GeocodeAsync(string address, int limit = 5)
        {
            return AutocompleteAsync(address, null, null, limit);
        }

        public Task<LocationDto?> ReverseGeocodeAsync(double lat, double lon)
        {
            // naive nearest by euclidean distance (sufficient for demo)
            var nearest = _data.OrderBy(d => DistanceSquared(d.Point.Latitude, d.Point.Longitude, lat, lon)).FirstOrDefault();
            return Task.FromResult(nearest);
        }

        public Task<IEnumerable<LocationDto>> NearbyAsync(double lat, double lon, int radiusMeters = 2000)
        {
            // return all for demo
            return Task.FromResult<IEnumerable<LocationDto>>(_data);
        }

        private static double DistanceSquared(double aLat, double aLon, double bLat, double bLon)
        {
            var dlat = aLat - bLat;
            var dlon = aLon - bLon;
            return dlat * dlat + dlon * dlon;
        }
    }
}
