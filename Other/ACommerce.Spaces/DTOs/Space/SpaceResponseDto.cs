using ACommerce.Spaces.Enums;

namespace ACommerce.Spaces.DTOs.Space;

public class SpaceResponseDto
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string Name { get; set; } = default!;
    public string Slug { get; set; } = default!;
    public SpaceType Type { get; set; }
    public string TypeName { get; set; } = default!;
    public SpaceStatus Status { get; set; }
    public string StatusName { get; set; } = default!;
    public string? ShortDescription { get; set; }
    public string? LongDescription { get; set; }
    public int Capacity { get; set; }
    public int MinCapacity { get; set; }
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
    public List<string> Images { get; set; } = new();
    public List<string> Videos { get; set; } = new();
    public string? VirtualTourUrl { get; set; }
    public bool InstantBooking { get; set; }
    public int MinBookingDurationMinutes { get; set; }
    public int? MaxBookingDurationMinutes { get; set; }
    public int AdvanceNoticeHours { get; set; }
    public string? CancellationPolicy { get; set; }
    public int? FreeCancellationHours { get; set; }
    public string? HouseRules { get; set; }
    public string? ContactPhone { get; set; }
    public string? ContactEmail { get; set; }
    public decimal AverageRating { get; set; }
    public int ReviewsCount { get; set; }
    public int CompletedBookingsCount { get; set; }
    public bool IsFeatured { get; set; }
    public bool IsVerified { get; set; }
    public List<string> Tags { get; set; } = new();

    // Summary counts
    public int AmenitiesCount { get; set; }
    public int PricesCount { get; set; }

    // Pricing summary
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public string? CurrencyCode { get; set; }
}
