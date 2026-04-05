using System.Collections.Generic;
using System.Threading.Tasks;
using ACommerce.DriverMatching.Models;

namespace ACommerce.DriverMatching
{
    public interface IDriverMatchingService
    {
        Task<IEnumerable<DriverMatchResult>> FindNearestDriversAsync(DriverMatchRequest request);
        Task<bool> UpdateDriverLocationAsync(string driverId, GeoPoint location, DriverStatus status);
    }
}
