namespace ACommerce.Catalog.Products.DTOs.ProductReview;

public class ProductReviewResponseDto
{
	public Guid Id { get; set; }
	public Guid ProductId { get; set; }
	public string UserId { get; set; } = string.Empty;
	public string UserName { get; set; } = string.Empty;
	public int Rating { get; set; }
	public string? Title { get; set; }
	public string? Comment { get; set; }
	public bool IsRecommended { get; set; }
	public bool IsVerifiedPurchase { get; set; }
	public bool IsApproved { get; set; }
	public List<string> Images { get; set; } = new();
	public int HelpfulVotes { get; set; }
	public int UnhelpfulVotes { get; set; }
	public DateTime CreatedAt { get; set; }
}

