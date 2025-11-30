using ACommerce.Client.Core.Http;

namespace ACommerce.Client.Orders;

/// <summary>
/// Client للتعامل مع Orders
/// </summary>
public sealed class OrdersClient
{
	private readonly IApiClient _httpClient;
	private const string ServiceName = "Marketplace";
	private const string BasePath = "/api/orders";

	public OrdersClient(IApiClient httpClient)
	{
		_httpClient = httpClient;
	}

	/// <summary>
	/// البحث في الطلبات (SmartSearch)
	/// </summary>
	public async Task<PagedOrderResult?> SearchAsync(
		OrderSearchRequest? request = null,
		CancellationToken cancellationToken = default)
	{
		request ??= new OrderSearchRequest();
		return await _httpClient.PostAsync<OrderSearchRequest, PagedOrderResult>(
			ServiceName,
			$"{BasePath}/search",
			request,
			cancellationToken);
	}

	/// <summary>
	/// الحصول على جميع الطلبات
	/// </summary>
	public async Task<List<OrderDto>?> GetAllAsync(CancellationToken cancellationToken = default)
	{
		var result = await SearchAsync(new OrderSearchRequest { PageSize = 100 }, cancellationToken);
		return result?.Items;
	}

	/// <summary>
	/// الحصول على طلباتي
	/// </summary>
	public async Task<List<OrderDto>?> GetMyOrdersAsync(
		string customerId,
		CancellationToken cancellationToken = default)
	{
		var result = await SearchAsync(new OrderSearchRequest
		{
			Filters = new List<OrderFilterItem>
			{
				new() { PropertyName = "CustomerId", Value = customerId, Operator = 0 }
			},
			PageSize = 50,
			OrderBy = "CreatedAt",
			Ascending = false
		}, cancellationToken);
		return result?.Items;
	}

	/// <summary>
	/// الحصول على طلب محدد
	/// </summary>
	public async Task<OrderDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
	{
		return await _httpClient.GetAsync<OrderDto>(
			ServiceName,
			$"{BasePath}/{id}",
			cancellationToken);
	}

	/// <summary>
	/// الحصول على طلب محدد (string id للتوافق)
	/// </summary>
	public async Task<OrderDto?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
	{
		return await _httpClient.GetAsync<OrderDto>(
			ServiceName,
			$"{BasePath}/{id}",
			cancellationToken);
	}

	/// <summary>
	/// الحصول على طلبات عميل محدد
	/// </summary>
	public async Task<PagedOrderResult?> GetCustomerOrdersAsync(
		string customerId,
		CancellationToken cancellationToken = default)
	{
		return await _httpClient.GetAsync<PagedOrderResult>(
			ServiceName,
			$"{BasePath}/customer/{customerId}",
			cancellationToken);
	}

	/// <summary>
	/// الحصول على طلبات بائع محدد
	/// </summary>
	public async Task<PagedOrderResult?> GetVendorOrdersAsync(
		Guid vendorId,
		CancellationToken cancellationToken = default)
	{
		return await _httpClient.GetAsync<PagedOrderResult>(
			ServiceName,
			$"{BasePath}/vendor/{vendorId}",
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
			BasePath,
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
	/// تأكيد طلب
	/// </summary>
	public async Task ConfirmAsync(Guid id, CancellationToken cancellationToken = default)
	{
		await _httpClient.PostAsync<object>(
			ServiceName,
			$"{BasePath}/{id}/confirm",
			new { },
			cancellationToken);
	}

	/// <summary>
	/// شحن طلب
	/// </summary>
	public async Task ShipAsync(
		Guid id,
		string trackingNumber,
		CancellationToken cancellationToken = default)
	{
		await _httpClient.PostAsync<string>(
			ServiceName,
			$"{BasePath}/{id}/ship",
			trackingNumber,
			cancellationToken);
	}

	/// <summary>
	/// إلغاء طلب
	/// </summary>
	public async Task CancelAsync(Guid id, string? reason = null, CancellationToken cancellationToken = default)
	{
		await _httpClient.PostAsync<string?>(
			ServiceName,
			$"{BasePath}/{id}/cancel",
			reason,
			cancellationToken);
	}

	/// <summary>
	/// إلغاء طلب (string id للتوافق)
	/// </summary>
	public async Task CancelAsync(string id, CancellationToken cancellationToken = default)
	{
		await _httpClient.PostAsync<object>(
			ServiceName,
			$"{BasePath}/{id}/cancel",
			new { },
			cancellationToken);
	}
}

// ===== Models =====

/// <summary>
/// نتيجة البحث المقسمة لصفحات
/// </summary>
public sealed class PagedOrderResult
{
	public List<OrderDto> Items { get; set; } = new();
	public int TotalCount { get; set; }
	public int PageNumber { get; set; }
	public int PageSize { get; set; }
	public int TotalPages { get; set; }
}

/// <summary>
/// طلب البحث في الطلبات
/// </summary>
public sealed class OrderSearchRequest
{
	public string? SearchTerm { get; set; }
	public List<OrderFilterItem>? Filters { get; set; }
	public int PageNumber { get; set; } = 1;
	public int PageSize { get; set; } = 20;
	public string? OrderBy { get; set; }
	public bool Ascending { get; set; } = true;
	public List<string>? IncludeProperties { get; set; }
	public bool IncludeDeleted { get; set; }
}

/// <summary>
/// عنصر فلترة
/// </summary>
public sealed class OrderFilterItem
{
	public string PropertyName { get; set; } = string.Empty;
	public object? Value { get; set; }
	public object? SecondValue { get; set; }
	public int Operator { get; set; }
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
