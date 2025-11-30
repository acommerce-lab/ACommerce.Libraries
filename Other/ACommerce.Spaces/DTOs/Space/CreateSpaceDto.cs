using ACommerce.Spaces.Enums;

namespace ACommerce.Spaces.DTOs.Space;

public class CreateSpaceDto
{
    public required string Name { get; set; }
    public required string Slug { get; set; }
    public SpaceType Type { get; set; } = SpaceType.CoWorking;
    public SpaceStatus Status { get; set; } = SpaceStatus.Draft;
    public string? ShortDescription { get; set; }
    public string? LongDescription { get; set; }
    public int Capacity { get; set; }
    public int MinCapacity { get; set; } = 1;
    public decimal? AreaSquareMeters { get; set; }
    public int? FloorNumber { get; set; }
    public string? RoomNumber { get; set; }
    public Guid OwnerId { get; set; }
    public Guid? LocationId { get; set; }
    public string? Address { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? City { get; set; }
    public string? District { get; set; }
    public string? PostalCode { get; set; }
    public string? FeaturedImage { get; set; }
    public List<string>? Images { get; set; }
    public List<string>? Videos { get; set; }
    public string? VirtualTourUrl { get; set; }
    public bool InstantBooking { get; set; } = true;
    public int MinBookingDurationMinutes { get; set; } = 60;
    public int? MaxBookingDurationMinutes { get; set; }
    public int AdvanceNoticeHours { get; set; } = 24;
    public string? CancellationPolicy { get; set; }
    public int? FreeCancellationHours { get; set; } = 24;
    public string? HouseRules { get; set; }
    public string? AccessInstructions { get; set; }
    public string? ContactPhone { get; set; }
    public string? ContactEmail { get; set; }
    public bool IsFeatured { get; set; }
    public List<string>? Tags { get; set; }
    public Dictionary<string, string>? Metadata { get; set; }
}
