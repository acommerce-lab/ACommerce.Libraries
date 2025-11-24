namespace ACommerce.Cart.DTOs;

public class AddToCartDto
{
	public required string UserIdOrSessionId { get; set; }
	public Guid ListingId { get; set; }
	public int Quantity { get; set; } = 1;
}

public class UpdateCartItemDto
{
	public int Quantity { get; set; }
}

public class CartResponseDto
{
	public Guid Id { get; set; }
	public string UserIdOrSessionId { get; set; } = string.Empty;
	public List<CartItemDto> Items { get; set; } = new();
	public string? CouponCode { get; set; }
	public decimal? DiscountAmount { get; set; }
	public decimal Subtotal { get; set; }
	public decimal Total { get; set; }
}

public class CartItemDto
{
	public Guid Id { get; set; }
	public Guid ListingId { get; set; }
	public int Quantity { get; set; }
	public decimal Price { get; set; }
	public decimal Total => Price * Quantity;
}
