namespace ACommerce.Payments.Noon.Models;

/// <summary>
/// خيارات بوابة نون للمدفوعات
/// </summary>
public class NoonOptions
{
	/// <summary>
	/// معرف التطبيق من لوحة تحكم نون
	/// </summary>
	public required string ApplicationIdentifier { get; set; }

	/// <summary>
	/// مفتاح التفويض
	/// </summary>
	public required string AuthorizationKey { get; set; }

	/// <summary>
	/// معرف الأعمال
	/// </summary>
	public required string BusinessIdentifier { get; set; }

	/// <summary>
	/// مفتاح API
	/// </summary>
	public required string ApiKey { get; set; }

	/// <summary>
	/// هل نستخدم بيئة الاختبار
	/// </summary>
	public bool UseSandbox { get; set; } = true;

	/// <summary>
	/// رابط API (يتم تحديده تلقائياً حسب UseSandbox)
	/// </summary>
	public string ApiUrl => UseSandbox
		? "https://api-stg.noonpayments.com/payment/v1"
		: "https://api.noonpayments.com/payment/v1";

	/// <summary>
	/// العملة الافتراضية
	/// </summary>
	public string DefaultCurrency { get; set; } = "SAR";

	/// <summary>
	/// فئة الطلب الافتراضية
	/// </summary>
	public string DefaultOrderCategory { get; set; } = "pay";

	/// <summary>
	/// قناة الدفع الافتراضية
	/// </summary>
	public string DefaultChannel { get; set; } = "web";

	/// <summary>
	/// رابط العودة بعد الدفع
	/// </summary>
	public string? ReturnUrl { get; set; }

	/// <summary>
	/// مهلة الاتصال بالثواني
	/// </summary>
	public int TimeoutSeconds { get; set; } = 30;
}
