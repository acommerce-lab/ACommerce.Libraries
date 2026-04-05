namespace ACommerce.DriverMatching.Models
{
    public class DriverMatchResult
    {
        public string DriverId { get; set; }
        public string Name { get; set; }
        public double DistanceMeters { get; set; }
        public int EtaSeconds { get; set; }
        public GeoPoint Location { get; set; }
        public string VehicleType { get; set; }
        public double Rating { get; set; }
    }
}
