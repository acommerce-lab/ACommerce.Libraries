namespace ACommerce.Orders.Enums;

/// <summary>
/// حالة الطلب
/// </summary>
public enum OrderStatus
{
	/// <summary>
	/// مسودة (في السلة)
	/// </summary>
	Draft = 1,

	/// <summary>
	/// قيد الانتظار (بانتظار الدفع)
	/// </summary>
	Pending = 2,

	/// <summary>
	/// مؤكد (تم الدفع)
	/// </summary>
	Confirmed = 3,

	/// <summary>
	/// قيد التجهيز
	/// </summary>
	Processing = 4,

	/// <summary>
	/// جاهز للشحن
	/// </summary>
	ReadyToShip = 5,

	/// <summary>
	/// تم الشحن
	/// </summary>
	Shipped = 6,

	/// <summary>
	/// قيد التوصيل
	/// </summary>
	OutForDelivery = 7,

	/// <summary>
	/// تم التسليم
	/// </summary>
	Delivered = 8,

	/// <summary>
	/// ملغي
	/// </summary>
	Cancelled = 9,

	/// <summary>
	/// مرتجع
	/// </summary>
	Returned = 10,

	/// <summary>
	/// مسترجع (Refunded)
	/// </summary>
	Refunded = 11
}
