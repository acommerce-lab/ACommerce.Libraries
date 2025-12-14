namespace ACommerce.Shipping.Abstractions.Enums;

/// <summary>
/// حالة الشحنة
/// </summary>
public enum ShipmentStatus
{
	Pending = 1,
	PickedUp = 2,
	InTransit = 3,
	OutForDelivery = 4,
	Delivered = 5,
	Failed = 6,
	Returned = 7,
	Cancelled = 8
}
