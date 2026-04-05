using System;
using ACommerce.DriverMatching.Models;

namespace ACommerce.RideLifecycle.Models
{
    public class Ride
    {
        public Guid Id { get; set; } = Guid.NewGuid();
    public required GeoPoint Pickup { get; set; }
        public GeoPoint? Dropoff { get; set; }
        public string? RiderId { get; set; }
        public string? AssignedDriverId { get; set; }
        public RideState State { get; set; } = RideState.Requested;
        public bool Paid { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ArrivalAt { get; set; }
        public int? Rating { get; set; }
        public string? RatingFeedback { get; set; }
    }
}
