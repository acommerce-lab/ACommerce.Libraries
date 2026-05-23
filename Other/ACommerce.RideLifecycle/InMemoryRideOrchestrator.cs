using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using ACommerce.RideLifecycle.Models;
using Microsoft.Extensions.DependencyInjection;
using ACommerce.DriverMatching.Models;
using ACommerce.DriverMatching;
using ACommerce.Notifications.Abstractions.Contracts;
using ACommerce.Notifications.Abstractions.Models;
using ACommerce.Chats.Abstractions.Providers;
using ACommerce.Chats.Abstractions.DTOs;

namespace ACommerce.RideLifecycle
{
    public class InMemoryRideOrchestrator : IRideOrchestrator
    {
        private readonly ConcurrentDictionary<Guid, Ride> _rides = new();
        private readonly IDriverMatchingService _matching;
        private readonly IServiceScopeFactory? _scopeFactory;

        public InMemoryRideOrchestrator(
            IDriverMatchingService matching,
            IServiceScopeFactory? scopeFactory = null)
        {
            _matching = matching;
            _scopeFactory = scopeFactory;
        }

        public async Task<Ride> RequestRideAsync(string riderId, GeoPoint pickup, GeoPoint? dropoff = null)
        {
            var ride = new Ride { RiderId = riderId, Pickup = pickup, Dropoff = dropoff };
            _rides[ride.Id] = ride;

            // try matching immediately
            var req = new DriverMatchRequest { PickupLocation = pickup, MaxResults = 5, RadiusMeters = 5000 };
            var matches = (await _matching.FindNearestDriversAsync(req)).ToList();
            if (matches.Any())
            {
                // assign the closest candidate optimistically
                var first = matches.First();
                ride.AssignedDriverId = first.DriverId;
                ride.State = Models.RideState.Matched;

                // publish a lightweight notification to the rider (if available)
                _ = PublishNotificationAsync(ride.RiderId, "Driver matched", $"Driver {first.DriverId} matched to your ride (id: {ride.Id}).");
                // notify driver about potential assignment
                _ = PublishNotificationAsync(first.DriverId, "New ride nearby", $"A rider requested a ride near you (ride id: {ride.Id}).");
            }
            else
            {
                // Development/demo fallback: if no matches were found, assign a demo driver so the local end-to-end
                // flow can proceed without needing geographically accurate seeds.
                // NOTE: this is a best-effort convenience for local testing only.
                var demoDriverId = "driver-1";
                ride.AssignedDriverId = demoDriverId;
                ride.State = Models.RideState.Matched;
                _ = PublishNotificationAsync(ride.RiderId, "Driver matched (demo)", $"Demo driver {demoDriverId} matched to your ride (id: {ride.Id}).");
                _ = PublishNotificationAsync(demoDriverId, "New ride nearby (demo)", $"A rider requested a ride (demo assignment: ride id {ride.Id}).");
            }

            _rides[ride.Id] = ride;

            // Development convenience: write a simple file-based export so other local processes
            // can pick up the ride when running a deterministic end-to-end demo. This is
            // intentionally developer-only and best-effort (no retries).
            try
            {
                var dir = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "rukkab-rides");
                System.IO.Directory.CreateDirectory(dir);
                var path = System.IO.Path.Combine(dir, ride.Id.ToString() + ".json");
                var json = System.Text.Json.JsonSerializer.Serialize(ride);
                System.IO.File.WriteAllText(path, json);
            }
            catch
            {
                // best-effort only
            }

            return ride;
        }

        public Task<Ride?> GetRideAsync(Guid rideId)
        {
            _rides.TryGetValue(rideId, out var ride);
            return Task.FromResult(ride);
        }

        public Task<System.Collections.Generic.List<Ride>> GetRidesForRiderAsync(string riderId)
        {
            var list = _rides.Values.Where(r => string.Equals(r.RiderId, riderId, StringComparison.OrdinalIgnoreCase)).ToList();
            return Task.FromResult(list);
        }

        public Task<bool> DriverAcceptAsync(Guid rideId, string driverId)
        {
            if (!_rides.TryGetValue(rideId, out var ride))
            {
                // Development fallback: if another local process exported the ride to a temp file,
                // try to import it automatically so local demos can proceed without HTTP imports.
                try
                {
                    var dir = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "rukkab-rides");
                    var path = System.IO.Path.Combine(dir, rideId.ToString() + ".json");
                    if (System.IO.File.Exists(path))
                    {
                        var txt = System.IO.File.ReadAllText(path);
                        var imported = System.Text.Json.JsonSerializer.Deserialize<Ride>(txt, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        if (imported != null)
                        {
                            _rides[imported.Id] = imported;
                            ride = imported;
                            try { System.Console.WriteLine($"[DEBUG] InMemoryRideOrchestrator: auto-imported ride from file {path}"); } catch { }
                        }
                    }
                }
                catch
                {
                    // ignore
                }
            }
            if (ride == null) return Task.FromResult(false);
            // allow acceptance if the ride was assigned to this driver, or if no assignment was made (development/demo convenience)
            if (string.IsNullOrEmpty(ride.AssignedDriverId))
            {
                ride.AssignedDriverId = driverId;
            }
            else if (ride.AssignedDriverId != driverId)
            {
                return Task.FromResult(false);
            }

            ride.State = Models.RideState.Assigned;
            _rides[rideId] = ride;

            // publish notifications
            _ = PublishNotificationAsync(ride.RiderId, "Driver accepted", $"Driver {driverId} accepted ride {ride.Id}.");
            _ = PublishNotificationAsync(driverId, "Ride assigned", $"You accepted ride {ride.Id}.");

            // create an in-ride chat between rider and driver (if chat provider is available)
            _ = EnsureRideChatAsync(ride, driverId);

            return Task.FromResult(true);
        }

        public Task<bool> StartRideAsync(Guid rideId)
        {
            if (!_rides.TryGetValue(rideId, out var ride)) return Task.FromResult(false);
            if (ride.State != Models.RideState.Assigned) return Task.FromResult(false);

            ride.State = Models.RideState.Active;
            _rides[rideId] = ride;

            _ = PublishNotificationAsync(ride.RiderId!, "Ride started", $"Your ride {ride.Id} has started.");
            if (!string.IsNullOrEmpty(ride.AssignedDriverId))
                _ = PublishNotificationAsync(ride.AssignedDriverId, "Ride in progress", $"Ride {ride.Id} is now active.");

            return Task.FromResult(true);
        }

        public Task<bool> CompleteRideAsync(Guid rideId)
        {
            if (!_rides.TryGetValue(rideId, out var ride)) return Task.FromResult(false);
            if (ride.State != Models.RideState.Active) return Task.FromResult(false);

            ride.State = Models.RideState.Completed;
            _rides[rideId] = ride;

            _ = PublishNotificationAsync(ride.RiderId!, "Ride completed", $"Your ride {ride.Id} is completed.");
            if (!string.IsNullOrEmpty(ride.AssignedDriverId))
                _ = PublishNotificationAsync(ride.AssignedDriverId, "Ride completed", $"You completed ride {ride.Id}.");

            return Task.FromResult(true);
        }

        public Task<bool> DriverArrivedAsync(Guid rideId, string driverId)
        {
            if (!_rides.TryGetValue(rideId, out var ride)) return Task.FromResult(false);
            if (ride.AssignedDriverId != driverId) return Task.FromResult(false);

            ride.ArrivalAt = DateTime.UtcNow;
            _rides[rideId] = ride;

            _ = PublishNotificationAsync(ride.RiderId, "Driver arrived", $"Driver {driverId} has arrived at pickup for ride {ride.Id}.");
            _ = PublishNotificationAsync(driverId, "Arrival recorded", $"You marked arrival for ride {ride.Id}.");

            return Task.FromResult(true);
        }

        public Task<bool> CancelRideAsync(Guid rideId, string? reason = null)
        {
            if (!_rides.TryGetValue(rideId, out var ride)) return Task.FromResult(false);
            if (ride.State == Models.RideState.Completed || ride.State == Models.RideState.Cancelled) return Task.FromResult(false);

            ride.State = Models.RideState.Cancelled;
            _rides[rideId] = ride;

            var msg = string.IsNullOrEmpty(reason) ? $"Ride {ride.Id} was cancelled." : $"Ride {ride.Id} was cancelled: {reason}";
            _ = PublishNotificationAsync(ride.RiderId!, "Ride cancelled", msg);
            if (!string.IsNullOrEmpty(ride.AssignedDriverId))
                _ = PublishNotificationAsync(ride.AssignedDriverId, "Ride cancelled", msg);

            return Task.FromResult(true);
        }

        public Task<bool> RateRideAsync(Guid rideId, string riderId, int rating, string? feedback = null)
        {
            if (!_rides.TryGetValue(rideId, out var ride)) return Task.FromResult(false);
            if (ride.RiderId != riderId) return Task.FromResult(false);
            if (ride.State != Models.RideState.Completed) return Task.FromResult(false);

            ride.Rating = rating;
            ride.RatingFeedback = feedback;
            _rides[rideId] = ride;

            _ = PublishNotificationAsync(ride.AssignedDriverId ?? string.Empty, "Ride rated", $"Ride {ride.Id} received rating {rating}.");

            return Task.FromResult(true);
        }

        public Task<bool> MarkPaidAsync(Guid rideId)
        {
            if (!_rides.TryGetValue(rideId, out var ride)) return Task.FromResult(false);
            ride.Paid = true;
            ride.State = Models.RideState.Paid;
            _rides[rideId] = ride;
            return Task.FromResult(true);
        }

        // Development/debug helper: import a ride into the in-memory store so other services
        // running locally can operate on it. This is intentionally simple and meant for
        // local demo/testing only; do not expose in production.
        public Task ImportRideAsync(Ride ride)
        {
            if (ride == null) return Task.CompletedTask;
            _rides[ride.Id] = ride;
            try
            {
                System.Console.WriteLine($"[DEBUG] InMemoryRideOrchestrator: imported ride {ride.Id}");
            }
            catch
            {
                // best-effort logging for local dev
            }
            return Task.CompletedTask;
        }

        private async Task PublishNotificationAsync(string? userId, string title, string message)
        {
            try
            {
                if (string.IsNullOrEmpty(userId)) return;
                if (_scopeFactory == null) return;

                using var scope = _scopeFactory.CreateScope();
                var publisher = scope.ServiceProvider.GetService<INotificationPublisher>();
                if (publisher == null) return;

                var evt = new NotificationEvent
                {
                    UserId = userId,
                    Title = title,
                    Message = message,
                    CreatedAt = DateTimeOffset.UtcNow
                };

                await publisher.PublishAsync(evt);
            }
            catch
            {
                // best-effort; swallow to keep orchestrator lightweight in-memory
            }
        }

        private async Task EnsureRideChatAsync(Ride ride, string driverId)
        {
            if (_scopeFactory == null) return;
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var chatProvider = scope.ServiceProvider.GetService<IChatProvider>();
                if (chatProvider == null) return;

                var title = $"Ride {ride.Id}";
                var create = new CreateChatDto
                {
                    Title = title,
                    CreatorUserId = ride.RiderId ?? driverId,
                    ParticipantUserIds = new List<string>()
                };

                if (!string.IsNullOrEmpty(ride.RiderId)) create.ParticipantUserIds.Add(ride.RiderId);
                if (!string.IsNullOrEmpty(driverId)) create.ParticipantUserIds.Add(driverId);

                await chatProvider.CreateChatAsync(create);
            }
            catch
            {
                // ignore errors for now
            }
        }
    }
}
