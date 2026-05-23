using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Rukkab.Driver.Api.Services
{
    public record DriverLocation
    {
        public string DriverId { get; init; } = string.Empty;
        public double Latitude { get; init; }
        public double Longitude { get; init; }
        public DateTime Timestamp { get; init; }
    }

    public interface IDriverLocationStore
    {
        void UpdateLocation(string driverId, DriverLocation location);
        bool TryGetLocation(string driverId, out DriverLocation? location);
        IReadOnlyCollection<DriverLocation> GetAllLocations();
    }

    public class InMemoryDriverLocationStore : IDriverLocationStore
    {
        private readonly ConcurrentDictionary<string, DriverLocation> _locations = new();

        public void UpdateLocation(string driverId, DriverLocation location)
        {
            _locations.AddOrUpdate(driverId, location, (_, _) => location);
        }

        public bool TryGetLocation(string driverId, out DriverLocation? location)
        {
            if (_locations.TryGetValue(driverId, out var loc))
            {
                location = loc;
                return true;
            }

            location = null;
            return false;
        }

        public IReadOnlyCollection<DriverLocation> GetAllLocations()
        {
            return _locations.Values.ToList().AsReadOnly();
        }
    }
}
