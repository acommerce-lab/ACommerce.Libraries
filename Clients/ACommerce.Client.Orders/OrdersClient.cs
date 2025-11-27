using ACommerce.Client.Core.Http;

namespace ACommerce.Client.Orders;

/// <summary>
/// Client للتعامل مع Orders
/// </summary>
public sealed class OrdersClient
{
	private readonly DynamicHttpClient _httpClient;
	private const string ServiceName = "Marketplace"; // أو "Orders" إذا كانت خدمة منفصلة

	public OrdersClient(DynamicHttpClient httpClient)
	{
		_httpClient = httpClient;
	}

	/// <summary>
	/// الحصول على جميع الطلبات
	/// </summary>
	public async Task<List<OrderDto>?> GetAllAsync(CancellationToken cancellationToken = default)
	{
		return await _httpClient.GetAsync<List<OrderDto>>(
			ServiceName,
			"/api/orders",
			cancellationToken);
	}

	/// <summary>
	/// الحصول على طلباتي
	/// </summary>
	public async Task<List<OrderDto>?> GetMyOrdersAsync(CancellationToken cancellationToken = default)
	{
		return await _httpClient.GetAsync<List<OrderDto>>(
			ServiceName,
			"/api/orders/me",
			cancellationToken);
	}

	/// <summary>
	/// الحصول على طلب محدد
	/// </summary>
	public async Task<OrderDto?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
	{
		return await _httpClient.GetAsync<OrderDto>(
			ServiceName,
			$"/api/orders/{id}",
			cancellationToken);
	}

	/// <summary>
	/// إنشاء طلب جديد
	/// </summary>
	public async Task<OrderDto?> CreateAsync(
		CreateOrderRequest request,
		CancellationToken cancellationToken = default)
	{
		return await _httpClient.PostAsync<CreateOrderRequest, OrderDto>(
			ServiceName,
			"/api/orders",
			request,
			cancellationToken);
	}

	/// <summary>
	/// إنشاء طلب جديد (اسم بديل)
	/// </summary>
	public Task<OrderDto?> CreateOrderAsync(
		CreateOrderRequest request,
		CancellationToken cancellationToken = default)
	{
		return CreateAsync(request, cancellationToken);
	}

	/// <summary>
	/// تحديث حالة طلب
	/// </summary>
	public async Task<OrderDto?> UpdateStatusAsync(
		string id,
		string newStatus,
		CancellationToken cancellationToken = default)
	{
		return await _httpClient.PatchAsync<UpdateStatusRequest, OrderDto>(
			ServiceName,
			$"/api/orders/{id}/status",
			new UpdateStatusRequest { Status = newStatus },
			cancellationToken);
	}

	/// <summary>
	/// إلغاء طلب
	/// </summary>
	public async Task CancelAsync(string id, CancellationToken cancellationToken = default)
	{
		await _httpClient.PostAsync<object>(
			ServiceName,
			$"/api/orders/{id}/cancel",
			new { },
			cancellationToken);
	}
}

/// <summary>
/// Order DTO for client-side use
/// </summary>
public sealed class OrderDto
{
	public Guid Id { get; set; }
	public string OrderNumber { get; set; } = string.Empty;
	public string Status { get; set; } = string.Empty;
	public string? CustomerId { get; set; }
	public decimal TotalAmount { get; set; }
	public string? Currency { get; set; }
	public string? ShippingAddress { get; set; }
	public List<OrderItemDto> Items { get; set; } = new();
	public DateTime CreatedAt { get; set; }
	public DateTime? UpdatedAt { get; set; }
}

public sealed class OrderItemDto
{
	public Guid Id { get; set; }
	public string ProductId { get; set; } = string.Empty;
	public string? ProductName { get; set; }
	public int Quantity { get; set; }
	public decimal UnitPrice { get; set; }
	public decimal TotalPrice { get; set; }
}

public sealed class CreateOrderRequest
{
	public List<OrderItemRequest>? Items { get; set; }
	public string? ShippingAddress { get; set; }
	public Guid? ShippingAddressId { get; set; }
	public Guid? ShippingMethodId { get; set; }
	public string? PaymentMethod { get; set; }
}

public sealed class OrderItemRequest
{
	public string ProductId { get; set; } = string.Empty;
	public int Quantity { get; set; }
	public decimal UnitPrice { get; set; }
}

public sealed class UpdateStatusRequest
{
	public string Status { get; set; } = string.Empty;
}
