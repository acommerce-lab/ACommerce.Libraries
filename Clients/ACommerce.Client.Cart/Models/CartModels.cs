namespace ACommerce.Client.Cart.Models;

public sealed class AddToCartRequest
{
	public required string UserIdOrSessionId { get; set; }
	public Guid ListingId { get; set; }
	public int Quantity { get; set; } = 1;
}

public sealed class UpdateCartItemRequest
{
	public int Quantity { get; set; }
}

public sealed class ApplyCouponRequest
{
	public string CouponCode { get; set; } = string.Empty;
}

public sealed class CartResponse
{
	public Guid Id { get; set; }
	public string UserIdOrSessionId { get; set; } = string.Empty;
	public List<CartItemResponse> Items { get; set; } = new();
	public string? CouponCode { get; set; }
	public decimal? DiscountAmount { get; set; }
	public decimal Subtotal { get; set; }
	public decimal Total { get; set; }
}

public sealed class CartItemResponse
{
	public Guid Id { get; set; }
	public Guid ListingId { get; set; }
	public int Quantity { get; set; }
	public decimal Price { get; set; }
	public decimal Total => Price * Quantity;
}
