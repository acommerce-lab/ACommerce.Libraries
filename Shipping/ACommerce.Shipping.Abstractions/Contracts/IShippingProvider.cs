using ACommerce.Shipping.Abstractions.Models;

namespace ACommerce.Shipping.Abstractions.Contracts;

/// <summary>
/// واجهة مزود الشحن
/// </summary>
public interface IShippingProvider
{
	/// <summary>
	/// اسم المزود
	/// </summary>
	string ProviderName { get; }

	/// <summary>
	/// إنشاء شحنة
	/// </summary>
	Task<ShipmentResult> CreateShipmentAsync(ShipmentRequest request, CancellationToken cancellationToken = default);

	/// <summary>
	/// تتبع الشحنة
	/// </summary>
	Task<TrackingInfo> TrackShipmentAsync(string trackingNumber, CancellationToken cancellationToken = default);

	/// <summary>
	/// إلغاء الشحنة
	/// </summary>
	Task<bool> CancelShipmentAsync(string shipmentId, CancellationToken cancellationToken = default);

	/// <summary>
	/// حساب تكلفة الشحن
	/// </summary>
	Task<decimal> CalculateShippingCostAsync(ShipmentRequest request, CancellationToken cancellationToken = default);
}
