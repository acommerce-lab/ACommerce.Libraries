using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Rukkab.Driver.Api.Services;

namespace Rukkab.Driver.Api.Hubs
{
    public class DriverLocationHub : Hub
    {
        private readonly IDriverLocationStore _store;

        public DriverLocationHub(IDriverLocationStore store)
        {
            _store = store;
        }

        /// <summary>
        /// Called by drivers to push their current location.
        /// The server stores the last-known location and broadcasts to other subscribers.
        /// </summary>
        public async Task UpdateLocation(string driverId, double latitude, double longitude, DateTime? timestamp = null)
        {
            var ts = timestamp ?? DateTime.UtcNow;

            var loc = new DriverLocation
            {
                DriverId = driverId,
                Latitude = latitude,
                Longitude = longitude,
                Timestamp = ts
            };

            _store.UpdateLocation(driverId, loc);

            // Broadcast to other connected clients (riders, matching dashboards, etc.)
            await Clients.Others.SendAsync("DriverLocationUpdated", loc.DriverId, loc.Latitude, loc.Longitude, loc.Timestamp);
        }
    }
}
