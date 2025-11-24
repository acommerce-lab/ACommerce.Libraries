namespace ACommerce.Vendors.Enums;

/// <summary>
/// حالة البائع
/// </summary>
public enum VendorStatus
{
	/// <summary>
	/// قيد المراجعة
	/// </summary>
	Pending = 1,

	/// <summary>
	/// نشط
	/// </summary>
	Active = 2,

	/// <summary>
	/// معلق
	/// </summary>
	Suspended = 3,

	/// <summary>
	/// محظور
	/// </summary>
	Banned = 4,

	/// <summary>
	/// غير نشط
	/// </summary>
	Inactive = 5
}
