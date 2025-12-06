using System.Text.Json.Serialization;

namespace ACommerce.Payments.Noon.Models;

/// <summary>
/// خيارات بوابة نون للمدفوعات
/// </summary>
public class NoonOptions
{
	/// <summary>
	/// معرف التطبيق من لوحة تحكم نون
	/// </summary>
	public string ApplicationIdentifier { get; set; } = string.Empty;

	/// <summary>
	/// مفتاح التفويض (AuthorizationKey أو ApplicationKey)
	/// </summary>
	public string AuthorizationKey { get; set; } = string.Empty;

	/// <summary>
	/// مفتاح التطبيق (بديل لـ AuthorizationKey)
	/// </summary>
	public string ApplicationKey
	{
		get => AuthorizationKey;
		set => AuthorizationKey = value;
	}

	/// <summary>
	/// معرف الأعمال
	/// </summary>
	public string BusinessIdentifier { get; set; } = string.Empty;

	/// <summary>
	/// مفتاح API (اختياري)
	/// </summary>
	public string? ApiKey { get; set; }

	/// <summary>
	/// هل نستخدم بيئة الاختبار
	/// </summary>
	public bool UseSandbox { get; set; } = true;

	/// <summary>
	/// بديل لـ UseSandbox
	/// </summary>
	public bool IsSandbox
	{
		get => UseSandbox;
		set => UseSandbox = value;
	}

	/// <summary>
	/// البيئة (Test أو Live) - بديل لـ UseSandbox
	/// </summary>
	public string? Environment
	{
		get => UseSandbox ? "Test" : "Live";
		set => UseSandbox = string.Equals(value, "Test", StringComparison.OrdinalIgnoreCase);
	}

	/// <summary>
	/// المنطقة (SA, AE, etc)
	/// </summary>
	public string Region { get; set; } = "SA";

	/// <summary>
	/// رابط API (يتم تحديده تلقائياً حسب UseSandbox والمنطقة)
	/// </summary>
	public string ApiUrl => GetApiUrl();

	private string GetApiUrl()
	{
		var regionPath = Region?.ToUpperInvariant() switch
		{
			"SA" => ".sa",
			"EG" => ".eg",
			_ => "" // Global endpoint
		};

		return UseSandbox
			? $"https://api-test{regionPath}.noonpayments.com/payment/v1"
			: $"https://api{regionPath}.noonpayments.com/payment/v1";
	}

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
