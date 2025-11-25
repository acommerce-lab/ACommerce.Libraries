using ACommerce.Client.Core.Http;

namespace ACommerce.Client.Shipping;

public sealed class ShippingClient
{
	private readonly DynamicHttpClient _httpClient;
	private const string ServiceName = "Shipping"; // أو "Marketplace"

	public ShippingClient(DynamicHttpClient httpClient)
	{
		_httpClient = httpClient;
	}

	/// <summary>
	/// حساب تكلفة الشحن
	/// </summary>
	public async Task<ShippingRateResponse?> CalculateShippingAsync(
		ShippingRateRequest request,
		CancellationToken cancellationToken = default)
	{
		return await _httpClient.PostAsync<ShippingRateRequest, ShippingRateResponse>(
			ServiceName,
			"/api/shipping/calculate",
			request,
			cancellationToken);
	}

	/// <summary>
	/// إنشاء شحنة
	/// </summary>
	public async Task<ShipmentResponse?> CreateShipmentAsync(
		CreateShipmentRequest request,
		CancellationToken cancellationToken = default)
	{
		return await _httpClient.PostAsync<CreateShipmentRequest, ShipmentResponse>(
			ServiceName,
			"/api/shipping/shipments",
			request,
			cancellationToken);
	}

	/// <summary>
	/// تتبع الشحنة
	/// </summary>
	public async Task<TrackingResponse?> TrackShipmentAsync(
		string trackingNumber,
		CancellationToken cancellationToken = default)
	{
		return await _httpClient.GetAsync<TrackingResponse>(
			ServiceName,
			$"/api/shipping/track/{trackingNumber}",
			cancellationToken);
	}

	/// <summary>
	/// الحصول على شركات الشحن المتاحة
	/// </summary>
	public async Task<List<ShippingProviderResponse>?> GetProvidersAsync(
		CancellationToken cancellationToken = default)
	{
		return await _httpClient.GetAsync<List<ShippingProviderResponse>>(
			ServiceName,
			"/api/shipping/providers",
			cancellationToken);
	}
}

public sealed class ShippingRateRequest
{
	public string FromCity { get; set; } = string.Empty;
	public string ToCity { get; set; } = string.Empty;
	public decimal Weight { get; set; } // بالكيلوجرام
	public string? ShippingProvider { get; set; }
}

public sealed class ShippingRateResponse
{
	public List<ShippingRate> Rates { get; set; } = new();
}

public sealed class ShippingRate
{
	public string Provider { get; set; } = string.Empty;
	public string ServiceType { get; set; } = string.Empty; // "Standard", "Express"
	public decimal Cost { get; set; }
	public int EstimatedDays { get; set; }
}

public sealed class CreateShipmentRequest
{
	public Guid OrderId { get; set; }
	public string ShippingProvider { get; set; } = string.Empty;
	public string ServiceType { get; set; } = string.Empty;
	public AddressInfo FromAddress { get; set; } = new();
	public AddressInfo ToAddress { get; set; } = new();
	public decimal Weight { get; set; }
}

public sealed class AddressInfo
{
	public string Name { get; set; } = string.Empty;
	public string Phone { get; set; } = string.Empty;
	public string Street { get; set; } = string.Empty;
	public string City { get; set; } = string.Empty;
	public string PostalCode { get; set; } = string.Empty;
	public string Country { get; set; } = "SA";
}

public sealed class ShipmentResponse
{
	public string ShipmentId { get; set; } = string.Empty;
	public string TrackingNumber { get; set; } = string.Empty;
	public Guid OrderId { get; set; }
	public string Status { get; set; } = string.Empty;
	public string ShippingProvider { get; set; } = string.Empty;
	public DateTime CreatedAt { get; set; }
}

public sealed class TrackingResponse
{
	public string TrackingNumber { get; set; } = string.Empty;
	public string Status { get; set; } = string.Empty;
	public List<TrackingEvent> Events { get; set; } = new();
	public DateTime? EstimatedDelivery { get; set; }
}

public sealed class TrackingEvent
{
	public string Status { get; set; } = string.Empty;
	public string Location { get; set; } = string.Empty;
	public string Description { get; set; } = string.Empty;
	public DateTime Timestamp { get; set; }
}

public sealed class ShippingProviderResponse
{
	public string ProviderId { get; set; } = string.Empty;
	public string Name { get; set; } = string.Empty;
	public List<string> ServiceTypes { get; set; } = new();
}
