using ACommerce.Client.Core.Http;
  using ACommerce.Orders.Entities;
using ACommerce.Orders.Enums;

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
	public async Task<List<Order>?> GetAllAsync(CancellationToken cancellationToken = default)
	{
		return await _httpClient.GetAsync<List<Order>>(
			ServiceName,
			"/api/orders",
			cancellationToken);
	}

	/// <summary>
	/// الحصول على طلب محدد
	/// </summary>
	public async Task<Order?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
	{
		return await _httpClient.GetAsync<Order>(
			ServiceName,
			$"/api/orders/{id}",
			cancellationToken);
	}

	/// <summary>
	/// إنشاء طلب جديد
	/// </summary>
	public async Task<Order?> CreateAsync(
		CreateOrderRequest request,
		CancellationToken cancellationToken = default)
	{
		return await _httpClient.PostAsync<CreateOrderRequest, Order>(
			ServiceName,
			"/api/orders",
			request,
			cancellationToken);
	}

	/// <summary>
	/// تحديث حالة طلب
	/// </summary>
	public async Task<Order?> UpdateStatusAsync(
		string id,
		OrderStatus newStatus,
		CancellationToken cancellationToken = default)
	{
		return await _httpClient.PatchAsync<UpdateStatusRequest, Order>(
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

public sealed class CreateOrderRequest
{
	public List<OrderItemRequest> Items { get; set; } = new();
	public string ShippingAddress { get; set; } = string.Empty;
}

public sealed class OrderItemRequest
{
	public string ProductId { get; set; } = string.Empty;
	public int Quantity { get; set; }
	public decimal UnitPrice { get; set; }
}

public sealed class UpdateStatusRequest
{
	public OrderStatus Status { get; set; }
}
