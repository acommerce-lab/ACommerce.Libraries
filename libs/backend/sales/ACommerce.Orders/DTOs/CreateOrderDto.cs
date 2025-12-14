namespace ACommerce.Orders.DTOs;

public class CreateOrderDto
{
	public required string CustomerId { get; set; }
	public required List<OrderItemDto> Items { get; set; }
	public string? CouponCode { get; set; }
	public required string ShippingAddress { get; set; }
	public string? BillingAddress { get; set; }
	public string? CustomerNotes { get; set; }
}

public class OrderItemDto
{
	public Guid ListingId { get; set; }
	public int Quantity { get; set; }
}

public class OrderResponseDto
{
	public Guid Id { get; set; }
	public string OrderNumber { get; set; } = string.Empty;
	public string CustomerId { get; set; } = string.Empty;
	public string Status { get; set; } = string.Empty;
	public decimal Subtotal { get; set; }
	public decimal DiscountAmount { get; set; }
	public decimal TaxAmount { get; set; }
	public decimal ShippingCost { get; set; }
	public decimal Total { get; set; }
	public string Currency { get; set; } = string.Empty;
	public List<OrderItemResponseDto> Items { get; set; } = new();
	public DateTime CreatedAt { get; set; }
}

public class OrderItemResponseDto
{
	public Guid Id { get; set; }
	public Guid ListingId { get; set; }
	public Guid ProductId { get; set; }
	public Guid VendorId { get; set; }
	public string ProductName { get; set; } = string.Empty;
	public int Quantity { get; set; }
	public decimal UnitPrice { get; set; }
	public decimal Total { get; set; }
}
