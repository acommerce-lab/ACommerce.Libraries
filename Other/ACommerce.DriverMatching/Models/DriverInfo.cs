namespace ACommerce.DriverMatching.Models
{
    public class DriverInfo
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public GeoPoint Location { get; set; }
        public DriverStatus Status { get; set; }
        public string VehicleType { get; set; }
        public double Rating { get; set; }
    }
}
