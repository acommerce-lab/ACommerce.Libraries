namespace ACommerce.Spaces.DTOs.Review;

public class CreateReviewDto
{
    public Guid SpaceId { get; set; }
    public Guid? BookingId { get; set; }
    public Guid ReviewerId { get; set; }
    public string? ReviewerName { get; set; }
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
}
