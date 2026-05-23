using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ACommerce.DriverMatching.Models;

namespace ACommerce.DriverMatching
{
    public class InMemoryDriverMatchingService : IDriverMatchingService
    {
        // simple concurrent store of drivers
        private readonly ConcurrentDictionary<string, DriverInfo> _drivers = new();

        public InMemoryDriverMatchingService()
        {
            // seed a few drivers
            Seed();
        }

        private void Seed()
        {
            var now = DateTime.UtcNow;
            // place driver-1 near the demo pickup coordinates used by the runner (New York)
            AddOrUpdateDriver(new DriverInfo { Id = "driver-1", Name = "Ali", Location = new GeoPoint(40.72, -73.995), Status = DriverStatus.Available, VehicleType = "Car", Rating = 4.8 });
            AddOrUpdateDriver(new DriverInfo { Id = "driver-2", Name = "Sara", Location = new GeoPoint(31.5215, 74.36), Status = DriverStatus.Available, VehicleType = "Car", Rating = 4.7 });
            AddOrUpdateDriver(new DriverInfo { Id = "driver-3", Name = "Hassan", Location = new GeoPoint(31.515, 74.35), Status = DriverStatus.Busy, VehicleType = "Bike", Rating = 4.4 });
        }

        private void AddOrUpdateDriver(DriverInfo d)
        {
            _drivers.AddOrUpdate(d.Id, d, (k, v) => d);
        }

        public Task<bool> UpdateDriverLocationAsync(string driverId, GeoPoint location, DriverStatus status)
        {
            _drivers.AddOrUpdate(driverId, id => new DriverInfo
            {
                Id = driverId,
                Name = driverId,
                Location = location,
                Status = status,
                VehicleType = "Car",
                Rating = 4.5
            }, (id, existing) =>
            {
                existing.Location = location;
                existing.Status = status;
                return existing;
            });

            return Task.FromResult(true);
        }

        public Task<IEnumerable<DriverMatchResult>> FindNearestDriversAsync(DriverMatchRequest request)
        {
            var results = new List<DriverMatchResult>();

            foreach (var kv in _drivers.Values)
            {
                if (kv.Status != DriverStatus.Available) continue;
                if (!string.IsNullOrEmpty(request.VehicleType) && !string.Equals(kv.VehicleType, request.VehicleType, StringComparison.OrdinalIgnoreCase)) continue;

                var dist = HaversineDistanceMeters(request.PickupLocation.Latitude, request.PickupLocation.Longitude, kv.Location.Latitude, kv.Location.Longitude);
                if (dist > request.RadiusMeters) continue;

                var eta = EstimateEtaSeconds(dist);
                results.Add(new DriverMatchResult
                {
                    DriverId = kv.Id,
                    Name = kv.Name,
                    DistanceMeters = dist,
                    EtaSeconds = eta,
                    Location = kv.Location,
                    VehicleType = kv.VehicleType,
                    Rating = kv.Rating
                });
            }

            var ordered = results.OrderBy(r => r.DistanceMeters).Take(request.MaxResults);
            return Task.FromResult(ordered.AsEnumerable());
        }

        private static double HaversineDistanceMeters(double lat1, double lon1, double lat2, double lon2)
        {
            double R = 6371000; // meters
            var dLat = ToRadians(lat2 - lat1);
            var dLon = ToRadians(lon2 - lon1);
            var a = Math.Sin(dLat/2) * Math.Sin(dLat/2) + Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) * Math.Sin(dLon/2) * Math.Sin(dLon/2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1-a));
            return R * c;
        }

        private static double ToRadians(double deg) => deg * (Math.PI / 180.0);

        private static int EstimateEtaSeconds(double meters)
        {
            // assume average speed 30 km/h = 8.33 m/s
            var speed = 8.33;
            return (int)Math.Max(30, meters / speed);
        }
    }
}
