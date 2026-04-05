using System.Text.Json.Serialization;

namespace ACommerce.Locations.Models
{
    public record PointDto(double Latitude, double Longitude);

    public record LocationDto
    {
        public string Id { get; init; } = default!;
        public string DisplayName { get; init; } = default!;
        public string Type { get; init; } = "place";
        public PointDto Point { get; init; } = new PointDto(0,0);
        public double Confidence { get; init; } = 1.0;
    }
}
