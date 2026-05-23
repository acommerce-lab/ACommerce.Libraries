using ACommerce.Locations.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ACommerce.Locations.Dev.Services
{
    // Development-only lightweight interface for the in-memory helper implementation.
    public interface IDevLocationService
    {
        Task<IEnumerable<LocationDto>> AutocompleteAsync(string query, double? lat = null, double? lon = null, int limit = 10);
        Task<IEnumerable<LocationDto>> GeocodeAsync(string address, int limit = 5);
        Task<LocationDto?> ReverseGeocodeAsync(double lat, double lon);
        Task<IEnumerable<LocationDto>> NearbyAsync(double lat, double lon, int radiusMeters = 2000);
    }
}
