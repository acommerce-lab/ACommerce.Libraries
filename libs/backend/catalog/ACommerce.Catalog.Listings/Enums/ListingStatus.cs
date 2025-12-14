using System.Text.Json.Serialization;

namespace ACommerce.Catalog.Listings.Enums;

/// <summary>
/// حالة العرض
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ListingStatus
{
	/// <summary>
	/// مسودة
	/// </summary>
	Draft = 1,

	/// <summary>
	/// قيد المراجعة
	/// </summary>
	PendingReview = 2,

	/// <summary>
	/// نشط
	/// </summary>
	Active = 3,

	/// <summary>
	/// غير متوفر
	/// </summary>
	OutOfStock = 4,

	/// <summary>
	/// معلق
	/// </summary>
	Suspended = 5,

	/// <summary>
	/// مرفوض
	/// </summary>
	Rejected = 6
}
