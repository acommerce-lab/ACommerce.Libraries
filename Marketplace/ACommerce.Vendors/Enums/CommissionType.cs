namespace ACommerce.Vendors.Enums;

/// <summary>
/// نوع العمولة
/// </summary>
public enum CommissionType
{
	/// <summary>
	/// نسبة مئوية
	/// </summary>
	Percentage = 1,

	/// <summary>
	/// مبلغ ثابت
	/// </summary>
	Fixed = 2,

	/// <summary>
	/// مختلط (نسبة + مبلغ)
	/// </summary>
	Hybrid = 3
}
