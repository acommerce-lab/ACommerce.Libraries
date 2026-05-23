using System;

namespace ACommerce.DriverMatching.Models
{
    public class DriverMatchRequest
    {
        public GeoPoint PickupLocation { get; set; }
        public int MaxResults { get; set; } = 5;
        public int RadiusMeters { get; set; } = 5000;
        public string VehicleType { get; set; }
        public Guid? RideId { get; set; }
    }
}
