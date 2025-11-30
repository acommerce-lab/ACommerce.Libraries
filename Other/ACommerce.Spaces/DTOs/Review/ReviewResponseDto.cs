namespace ACommerce.Spaces.DTOs.Review;

public class ReviewResponseDto
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid SpaceId { get; set; }
    public Guid? BookingId { get; set; }
    public Guid ReviewerId { get; set; }
    public string? ReviewerName { get; set; }
    public string? ReviewerAvatar { get; set; }
    public int Rating { get; set; }
    public int? CleanlinessRating { get; set; }
    public int? LocationRating { get; set; }
    public int? AmenitiesRating { get; set; }
    public int? ValueRating { get; set; }
    public int? CommunicationRating { get; set; }
    public string? Title { get; set; }
    public string? Comment { get; set; }
    public string? Pros { get; set; }
    public string? Cons { get; set; }
    public string? OwnerResponse { get; set; }
    public DateTime? OwnerResponseAt { get; set; }
    public bool IsVerified { get; set; }
    public int HelpfulCount { get; set; }
}

public class OwnerResponseDto
{
    public string Response { get; set; } = default!;
}
