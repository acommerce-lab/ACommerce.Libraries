namespace ACommerce.Reviews.DTOs;

public class CreateReviewDto
{
	public required string EntityType { get; set; }
	public required Guid EntityId { get; set; }
	public required string UserId { get; set; }
	public required int Rating { get; set; }
	public string? Title { get; set; }
	public string? Comment { get; set; }
	public List<string>? Pros { get; set; }
	public List<string>? Cons { get; set; }
	public List<string>? Images { get; set; }
}

public class ReviewResponseDto
{
	public Guid Id { get; set; }
	public string EntityType { get; set; } = string.Empty;
	public Guid EntityId { get; set; }
	public string UserId { get; set; } = string.Empty;
	public int Rating { get; set; }
	public string? Title { get; set; }
	public string? Comment { get; set; }
	public List<string> Pros { get; set; } = new();
	public List<string> Cons { get; set; } = new();
	public List<string> Images { get; set; } = new();
	public bool IsVerifiedPurchase { get; set; }
	public bool IsApproved { get; set; }
	public int HelpfulCount { get; set; }
	public string? VendorResponse { get; set; }
	public DateTime CreatedAt { get; set; }
}
