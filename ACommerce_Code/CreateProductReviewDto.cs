namespace ACommerce.Catalog.Products.DTOs.ProductReview;

public class CreateProductReviewDto
{
	public Guid ProductId { get; set; }
	public required string UserId { get; set; }
	public required string UserName { get; set; }
	public int Rating { get; set; }
	public string? Title { get; set; }
	public string? Comment { get; set; }
	public bool IsRecommended { get; set; }
	public bool IsVerifiedPurchase { get; set; }
	public List<string>? Images { get; set; }
}

