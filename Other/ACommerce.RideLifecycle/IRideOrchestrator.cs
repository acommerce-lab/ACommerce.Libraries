using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ACommerce.RideLifecycle.Models;
using ACommerce.DriverMatching.Models;

namespace ACommerce.RideLifecycle
{
    public interface IRideOrchestrator
    {
        Task<Ride> RequestRideAsync(string riderId, GeoPoint pickup, GeoPoint? dropoff = null);
        Task<Ride?> GetRideAsync(Guid rideId);
    Task<System.Collections.Generic.List<Ride>> GetRidesForRiderAsync(string riderId);
        Task<bool> DriverAcceptAsync(Guid rideId, string driverId);
        Task<bool> StartRideAsync(Guid rideId);
        Task<bool> CompleteRideAsync(Guid rideId);
        Task<bool> CancelRideAsync(Guid rideId, string? reason = null);
    Task<bool> DriverArrivedAsync(Guid rideId, string driverId);
    Task<bool> RateRideAsync(Guid rideId, string riderId, int rating, string? feedback = null);
        Task<bool> MarkPaidAsync(Guid rideId);
    }
}
