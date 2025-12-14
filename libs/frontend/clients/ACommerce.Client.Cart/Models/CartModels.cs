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
	public decimal SubTotal => Subtotal; // Alias for compatibility
	public decimal ShippingAmount { get; set; }
	public decimal TaxAmount { get; set; }
	public decimal Total { get; set; }
}

public sealed class CartItemResponse
{
	public Guid Id { get; set; }
	public Guid ListingId { get; set; }
	public string ProductName { get; set; } = string.Empty;
	public string? ImageUrl { get; set; }
	public string? VendorName { get; set; }
	public int Quantity { get; set; }
	public int MaxQuantity { get; set; } = 99;
	public decimal UnitPrice { get; set; }
	public decimal Price => UnitPrice; // Alias for compatibility
	public decimal TotalPrice => UnitPrice * Quantity;
	public decimal Total => TotalPrice; // Alias for compatibility
	public Dictionary<string, object> Properties { get; set; } = new();
}
