using System;
using ACommerce.DriverMatching.Models;

namespace ACommerce.RideLifecycle.Persistence
{
    public class RideEntity
    {
        public Guid Id { get; set; }
        public double PickupLatitude { get; set; }
        public double PickupLongitude { get; set; }
        public double? DropoffLatitude { get; set; }
        public double? DropoffLongitude { get; set; }
        public string? RiderId { get; set; }
        public string? AssignedDriverId { get; set; }
        public int State { get; set; }
        public bool Paid { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ArrivalAt { get; set; }
        public int? Rating { get; set; }
        public string? RatingFeedback { get; set; }
    }
}
