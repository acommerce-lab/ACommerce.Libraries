namespace ACommerce.Accounting.Core.Enums;

/// <summary>
/// ???? ????? ????????
/// </summary>
public enum EntryStatus
{
	/// <summary>
	/// ?????
	/// </summary>
	Draft = 1,

	/// <summary>
	/// ?????
	/// </summary>
	Approved = 2,

	/// <summary>
	/// ????? (Posted)
	/// </summary>
	Posted = 3,

	/// <summary>
	/// ????? (Reversed)
	/// </summary>
	Reversed = 4,

	/// <summary>
	/// ????
	/// </summary>
	Cancelled = 5
}

