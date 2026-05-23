using System;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;
using ACommerce.DriverMatching;
using ACommerce.DriverMatching.Models;
using ACommerce.Notifications.Abstractions.Contracts;
using ACommerce.Notifications.Abstractions.Models;
using ACommerce.Chats.Abstractions.DTOs;
using ACommerce.Chats.Abstractions.Providers;
using Microsoft.Extensions.DependencyInjection;
using ACommerce.RideLifecycle.Models;
using ACommerce.SharedKernel.Abstractions.Repositories;
using ACommerce.Catalog.Products.Entities;
using ACommerce.Catalog.Products.Enums;

namespace ACommerce.RideLifecycle.Persistence
{
    /// <summary>
    /// Persistent orchestrator that stores rides as catalog Products.
    /// This follows the user's instruction to treat rides as products and reuse the
    /// catalog/product infrastructure rather than introducing a new Ride entity.
    ///
    /// Implementation notes:
    /// - Ride metadata is serialized into Product.ShortDescription (JSON).
    /// - RiderId is stored in Product.Barcode to allow efficient querying by rider.
    /// - The shared IBaseAsyncRepository<Product> is used for persistence via IRepositoryFactory.
    /// </summary>
    public class PersistentRideOrchestrator : IRideOrchestrator
    {
        private readonly IRepositoryFactory _repoFactory;
        private readonly IDriverMatchingService _matching;
        private readonly IServiceScopeFactory? _scopeFactory;

        public PersistentRideOrchestrator(IRepositoryFactory repoFactory, IDriverMatchingService matching, IServiceScopeFactory? scopeFactory = null)
        {
            _repoFactory = repoFactory ?? throw new ArgumentNullException(nameof(repoFactory));
            _matching = matching;
            _scopeFactory = scopeFactory;
        }

        private record RidePayload
        {
            public string? RiderId { get; init; }
            public string? AssignedDriverId { get; set; }
            public int State { get; set; }
            public bool Paid { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime? ArrivalAt { get; set; }
            public int? Rating { get; set; }
            public string? RatingFeedback { get; set; }
            public double PickupLatitude { get; init; }
            public double PickupLongitude { get; init; }
            public double? DropoffLatitude { get; init; }
            public double? DropoffLongitude { get; init; }
        }

        private static Ride ProductToRide(Product p)
        {
            if (p == null) return null!;
            RidePayload? payload = null;
            try { payload = string.IsNullOrEmpty(p.ShortDescription) ? null : JsonSerializer.Deserialize<RidePayload>(p.ShortDescription); } catch { }

            var ride = new Ride
            {
                Id = p.Id,
                RiderId = payload?.RiderId ?? p.Barcode,
                AssignedDriverId = payload?.AssignedDriverId,
                State = payload != null ? (RideState)payload.State : RideState.Requested,
                Paid = payload?.Paid ?? false,
                CreatedAt = payload?.CreatedAt ?? p.CreatedAt,
                ArrivalAt = payload?.ArrivalAt,
                Rating = payload?.Rating ?? 0,
                RatingFeedback = payload?.RatingFeedback,
                Pickup = payload != null ? new GeoPoint(payload.PickupLatitude, payload.PickupLongitude) : new GeoPoint(0, 0),
                Dropoff = payload?.DropoffLatitude != null ? new GeoPoint(payload.DropoffLatitude.Value, payload.DropoffLongitude!.Value) : null
            };

            return ride;
        }

        private static void UpdateProductPayload(Product p, Ride r)
        {
            var payload = new RidePayload
            {
                RiderId = r.RiderId,
                AssignedDriverId = r.AssignedDriverId,
                State = (int)r.State,
                Paid = r.Paid,
                CreatedAt = r.CreatedAt == default ? DateTime.UtcNow : r.CreatedAt,
                ArrivalAt = r.ArrivalAt,
                Rating = r.Rating,
                RatingFeedback = r.RatingFeedback,
                PickupLatitude = r.Pickup.Latitude,
                PickupLongitude = r.Pickup.Longitude,
                DropoffLatitude = r.Dropoff?.Latitude,
                DropoffLongitude = r.Dropoff?.Longitude
            };

            p.ShortDescription = JsonSerializer.Serialize(payload);
            p.Barcode = r.RiderId; // store rider id for quick queries
        }

        public async Task<Ride> RequestRideAsync(string riderId, GeoPoint pickup, GeoPoint? dropoff = null)
        {
            var repo = _repoFactory.CreateRepository<Product>();

            var ride = new Ride { RiderId = riderId, Pickup = pickup, Dropoff = dropoff, CreatedAt = DateTime.UtcNow };

            var product = new Product
            {
                Name = $"Ride {Guid.NewGuid():N}",
                Sku = $"ride-{Guid.NewGuid():N}",
                Type = ProductType.Simple,
                Status = ProductStatus.Active
            };

            UpdateProductPayload(product, ride);

            var added = await repo.AddAsync(product);

            // try matching
            var req = new DriverMatchRequest { PickupLocation = pickup, MaxResults = 5, RadiusMeters = 5000 };
            var matches = (await _matching.FindNearestDriversAsync(req)).ToList();
            if (matches.Any())
            {
                var first = matches.First();
                ride.AssignedDriverId = first.DriverId;
                ride.State = RideState.Matched;

                // update product
                UpdateProductPayload(added, ride);
                await repo.UpdateAsync(added);

                _ = PublishNotificationAsync(ride.RiderId, "Driver matched", $"Driver {first.DriverId} matched to your ride (id: {added.Id}).");
                _ = PublishNotificationAsync(first.DriverId, "New ride nearby", $"A rider requested a ride near you (ride id: {added.Id}).");
            }
            else
            {
                // fallback demo convenience: assign a default driver if none found
                var demoDriverId = "driver-1";
                ride.AssignedDriverId = demoDriverId;
                ride.State = RideState.Matched;

                UpdateProductPayload(added, ride);
                await repo.UpdateAsync(added);

                _ = PublishNotificationAsync(ride.RiderId, "Driver matched (fallback)", $"Driver {demoDriverId} matched to your ride (id: {added.Id}).");
                _ = PublishNotificationAsync(demoDriverId, "New ride nearby (fallback)", $"A rider requested a ride (ride id {added.Id}).");
            }

            return ProductToRide(added);
        }

        public async Task<Ride?> GetRideAsync(Guid rideId)
        {
            var repo = _repoFactory.CreateRepository<Product>();
            var p = await repo.GetByIdAsync(rideId);
            if (p == null) return null;
            return ProductToRide(p);
        }

        public async Task<System.Collections.Generic.List<Ride>> GetRidesForRiderAsync(string riderId)
        {
            var repo = _repoFactory.CreateRepository<Product>();
            var items = await repo.GetAllWithPredicateAsync(p => p.Barcode == riderId);
            return items.Select(ProductToRide).ToList();
        }

        public async Task<bool> DriverAcceptAsync(Guid rideId, string driverId)
        {
            var repo = _repoFactory.CreateRepository<Product>();
            var p = await repo.GetByIdAsync(rideId, false);
            if (p == null) return false;

            var ride = ProductToRide(p);
            if (string.IsNullOrEmpty(ride.AssignedDriverId)) ride.AssignedDriverId = driverId;
            else if (ride.AssignedDriverId != driverId) return false;

            ride.State = RideState.Assigned;
            UpdateProductPayload(p, ride);
            await repo.UpdateAsync(p);

            _ = PublishNotificationAsync(ride.RiderId, "Driver accepted", $"Driver {driverId} accepted ride {p.Id}.");
            _ = PublishNotificationAsync(driverId, "Ride assigned", $"You accepted ride {p.Id}.");
            _ = EnsureRideChatAsync(ride, driverId);
            return true;
        }

        public async Task<bool> StartRideAsync(Guid rideId)
        {
            var repo = _repoFactory.CreateRepository<Product>();
            var p = await repo.GetByIdAsync(rideId, false);
            if (p == null) return false;
            var ride = ProductToRide(p);
            if (ride.State != RideState.Assigned) return false;
            ride.State = RideState.Active;
            UpdateProductPayload(p, ride);
            await repo.UpdateAsync(p);
            _ = PublishNotificationAsync(ride.RiderId, "Ride started", $"Your ride {p.Id} has started.");
            return true;
        }

        public async Task<bool> CompleteRideAsync(Guid rideId)
        {
            var repo = _repoFactory.CreateRepository<Product>();
            var p = await repo.GetByIdAsync(rideId, false);
            if (p == null) return false;
            var ride = ProductToRide(p);
            if (ride.State != RideState.Active) return false;
            ride.State = RideState.Completed;
            UpdateProductPayload(p, ride);
            await repo.UpdateAsync(p);
            _ = PublishNotificationAsync(ride.RiderId, "Ride completed", $"Your ride {p.Id} is completed.");
            return true;
        }

        public async Task<bool> DriverArrivedAsync(Guid rideId, string driverId)
        {
            var repo = _repoFactory.CreateRepository<Product>();
            var p = await repo.GetByIdAsync(rideId, false);
            if (p == null) return false;
            var ride = ProductToRide(p);
            if (ride.AssignedDriverId != driverId) return false;
            ride.ArrivalAt = DateTime.UtcNow;
            UpdateProductPayload(p, ride);
            await repo.UpdateAsync(p);
            _ = PublishNotificationAsync(ride.RiderId, "Driver arrived", $"Driver {driverId} has arrived at pickup for ride {p.Id}.");
            return true;
        }

        public async Task<bool> CancelRideAsync(Guid rideId, string? reason = null)
        {
            var repo = _repoFactory.CreateRepository<Product>();
            var p = await repo.GetByIdAsync(rideId, false);
            if (p == null) return false;
            var ride = ProductToRide(p);
            if (ride.State == RideState.Completed || ride.State == RideState.Cancelled) return false;
            ride.State = RideState.Cancelled;
            UpdateProductPayload(p, ride);
            await repo.UpdateAsync(p);
            _ = PublishNotificationAsync(ride.RiderId, "Ride cancelled", reason ?? $"Ride {p.Id} was cancelled.");
            return true;
        }

        public async Task<bool> RateRideAsync(Guid rideId, string riderId, int rating, string? feedback = null)
        {
            var repo = _repoFactory.CreateRepository<Product>();
            var p = await repo.GetByIdAsync(rideId, false);
            if (p == null) return false;
            var ride = ProductToRide(p);
            if (ride.RiderId != riderId) return false;
            if (ride.State != RideState.Completed) return false;
            ride.Rating = rating;
            ride.RatingFeedback = feedback;
            UpdateProductPayload(p, ride);
            await repo.UpdateAsync(p);
            _ = PublishNotificationAsync(ride.AssignedDriverId ?? string.Empty, "Ride rated", $"Ride {p.Id} received rating {rating}.");
            return true;
        }

        public async Task<bool> MarkPaidAsync(Guid rideId)
        {
            var repo = _repoFactory.CreateRepository<Product>();
            var p = await repo.GetByIdAsync(rideId, false);
            if (p == null) return false;
            var ride = ProductToRide(p);
            ride.Paid = true;
            ride.State = RideState.Paid;
            UpdateProductPayload(p, ride);
            await repo.UpdateAsync(p);
            return true;
        }

        public async Task ImportRideAsync(Ride ride)
        {
            if (ride == null) return;
            var repo = _repoFactory.CreateRepository<Product>();
            var exists = await repo.GetByIdAsync(ride.Id, false);
            if (exists == null)
            {
                var product = new Product
                {
                    Id = ride.Id,
                    Name = $"Ride {ride.Id:N}",
                    Sku = $"ride-{ride.Id:N}",
                    Type = ProductType.Simple,
                    Status = ProductStatus.Active
                };
                UpdateProductPayload(product, ride);
                await repo.AddAsync(product);
            }
            else
            {
                UpdateProductPayload(exists, ride);
                await repo.UpdateAsync(exists);
            }
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
                var evt = new NotificationEvent { UserId = userId, Title = title, Message = message, CreatedAt = DateTimeOffset.UtcNow };
                await publisher.PublishAsync(evt);
            }
            catch { }
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
                var create = new CreateChatDto { Title = title, CreatorUserId = ride.RiderId ?? driverId, ParticipantUserIds = new System.Collections.Generic.List<string>() };
                if (!string.IsNullOrEmpty(ride.RiderId)) create.ParticipantUserIds.Add(ride.RiderId);
                if (!string.IsNullOrEmpty(driverId)) create.ParticipantUserIds.Add(driverId);
                await chatProvider.CreateChatAsync(create);
            }
            catch { }
        }
    }
}
