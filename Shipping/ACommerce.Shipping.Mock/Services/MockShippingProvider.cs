using ACommerce.Shipping.Abstractions.Contracts;
using ACommerce.Shipping.Abstractions.Models;
using ACommerce.Shipping.Abstractions.Enums;

namespace ACommerce.Shipping.Mock.Services;

/// <summary>
/// مزود شحن وهمي للاختبار
/// يمكن استبداله بـ Aramex, SMSA, DHL, etc
/// </summary>
public class MockShippingProvider : IShippingProvider
{
	public string ProviderName => "Mock Shipping";

	public Task<ShipmentResult> CreateShipmentAsync(
		ShipmentRequest request,
		CancellationToken cancellationToken = default)
	{
		// Simulate shipment creation
		var trackingNumber = $"MOCK{DateTime.UtcNow:yyyyMMddHHmmss}";

		return Task.FromResult(new ShipmentResult
		{
			Success = true,
			TrackingNumber = trackingNumber,
			ShipmentId = Guid.NewGuid().ToString(),
			LabelUrl = $"https://example.com/labels/{trackingNumber}",
			Cost = CalculateCost(request)
		});
	}

	public Task<TrackingInfo> TrackShipmentAsync(
		string trackingNumber,
		CancellationToken cancellationToken = default)
	{
		// Simulate tracking
		return Task.FromResult(new TrackingInfo
		{
			TrackingNumber = trackingNumber,
			Status = ShipmentStatus.InTransit,
			CurrentLocation = "Distribution Center",
			EstimatedDelivery = DateTime.UtcNow.AddDays(2),
			Events = new List<TrackingEvent>
			{
				new() { Timestamp = DateTime.UtcNow.AddHours(-2), Description = "Picked up", Location = "Sender Address" },
				new() { Timestamp = DateTime.UtcNow.AddHours(-1), Description = "In transit", Location = "Distribution Center" }
			}
		});
	}

	public Task<bool> CancelShipmentAsync(
		string shipmentId,
		CancellationToken cancellationToken = default)
	{
		// Simulate cancellation
		return Task.FromResult(true);
	}

	public Task<decimal> CalculateShippingCostAsync(
		ShipmentRequest request,
		CancellationToken cancellationToken = default)
	{
		return Task.FromResult(CalculateCost(request));
	}

	private static decimal CalculateCost(ShipmentRequest request)
	{
		// Simple calculation based on weight
		var totalWeight = request.Packages.Sum(p => p.Weight);
		var baseCost = 20m; // SAR
		var weightCost = totalWeight * 2m; // 2 SAR per kg
		return baseCost + weightCost;
	}
}
