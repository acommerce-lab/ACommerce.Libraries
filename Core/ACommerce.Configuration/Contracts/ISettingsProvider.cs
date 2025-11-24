namespace ACommerce.Configuration.Contracts;

/// <summary>
/// واجهة مزود الإعدادات
/// </summary>
public interface ISettingsProvider
{
	/// <summary>
	/// الحصول على إعداد
	/// </summary>
	Task<T?> GetAsync<T>(string key, string scope = "Global", Guid? scopeId = null, CancellationToken cancellationToken = default);

	/// <summary>
	/// حفظ إعداد
	/// </summary>
	Task SaveAsync<T>(string key, T value, string scope = "Global", Guid? scopeId = null, CancellationToken cancellationToken = default);

	/// <summary>
	/// حذف إعداد
	/// </summary>
	Task DeleteAsync(string key, string scope = "Global", Guid? scopeId = null, CancellationToken cancellationToken = default);

	/// <summary>
	/// الحصول على جميع إعدادات نطاق معين
	/// </summary>
	Task<Dictionary<string, string>> GetAllAsync(string scope = "Global", Guid? scopeId = null, CancellationToken cancellationToken = default);
}

/// <summary>
/// إعدادات المتجر
/// </summary>
public class StoreSettings
{
	public string StoreName { get; set; } = string.Empty;
	public string StoreUrl { get; set; } = string.Empty;
	public string Logo { get; set; } = string.Empty;
	public string DefaultCurrency { get; set; } = "SAR";
	public string DefaultLanguage { get; set; } = "ar";
	public bool AllowGuestCheckout { get; set; } = true;
	public bool AutoApproveVendors { get; set; }
	public bool AutoApproveProducts { get; set; }
	public decimal DefaultCommissionRate { get; set; } = 10;
	public int MinimumOrderAmount { get; set; }
	public bool EnableMultiVendor { get; set; } = true;
}

/// <summary>
/// إعدادات البائع
/// </summary>
public class VendorSettings
{
	public Guid VendorId { get; set; }
	public bool EnableNotifications { get; set; } = true;
	public bool AutoConfirmOrders { get; set; }
	public int ProcessingTime { get; set; } = 3; // أيام
	public List<string> AllowedPaymentMethods { get; set; } = new();
	public List<string> AllowedShippingProviders { get; set; } = new();
	public Dictionary<string, string> CustomSettings { get; set; } = new();
}
