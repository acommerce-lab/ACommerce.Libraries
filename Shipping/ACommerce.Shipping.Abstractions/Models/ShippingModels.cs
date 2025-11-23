using ACommerce.Shipping.Abstractions.Enums;

namespace ACommerce.Shipping.Abstractions.Models;

/// <summary>
/// عنوان
/// </summary>
public record Address
{
	public required string FullName { get; init; }
	public required string Phone { get; init; }
	public string? Email { get; init; }
	public required string AddressLine1 { get; init; }
	public string? AddressLine2 { get; init; }
	public required string City { get; init; }
	public string? State { get; init; }
	public required string Country { get; init; }
	public required string PostalCode { get; init; }
}

/// <summary>
/// طلب شحن
/// </summary>
public record ShipmentRequest
{
	public required string OrderId { get; init; }
	public required Address FromAddress { get; init; }
	public required Address ToAddress { get; init; }
	public required List<Package> Packages { get; init; }
	public string? ServiceType { get; init; }
	public Dictionary<string, string> Metadata { get; init; } = new();
}

/// <summary>
/// طرد
/// </summary>
public record Package
{
	public decimal Weight { get; init; }
	public decimal? Length { get; init; }
	public decimal? Width { get; init; }
	public decimal? Height { get; init; }
	public string? Description { get; init; }
}

/// <summary>
/// نتيجة إنشاء شحنة
/// </summary>
public record ShipmentResult
{
	public required bool Success { get; init; }
	public required string TrackingNumber { get; init; }
	public required string ShipmentId { get; init; }
	public string? LabelUrl { get; init; }
	public decimal? Cost { get; init; }
	public string? ErrorMessage { get; init; }
}

/// <summary>
/// معلومات التتبع
/// </summary>
public record TrackingInfo
{
	public required string TrackingNumber { get; init; }
	public required ShipmentStatus Status { get; init; }
	public string? CurrentLocation { get; init; }
	public DateTime? EstimatedDelivery { get; init; }
	public List<TrackingEvent> Events { get; init; } = new();
}

/// <summary>
/// حدث تتبع
/// </summary>
public record TrackingEvent
{
	public required DateTime Timestamp { get; init; }
	public required string Description { get; init; }
	public string? Location { get; init; }
}
