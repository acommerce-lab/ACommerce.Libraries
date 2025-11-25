namespace ACommerce.ServiceRegistry.Abstractions.Models;

/// <summary>
/// استعلام عن خدمة
/// </summary>
public sealed class ServiceQuery
{
	/// <summary>
	/// اسم الخدمة (مطلوب)
	/// </summary>
	public string ServiceName { get; set; } = string.Empty;

	/// <summary>
	/// إصدار محدد (اختياري) - إن لم يُحدد سيأخذ أحدث إصدار
	/// </summary>
	public string? Version { get; set; }

	/// <summary>
	/// البيئة المطلوبة (اختياري)
	/// </summary>
	public string? Environment { get; set; }

	/// <summary>
	/// هل تريد فقط الخدمات السليمة؟ (افتراضياً: نعم)
	/// </summary>
	public bool OnlyHealthy { get; set; } = true;

	/// <summary>
	/// Tags للتصفية (اختياري)
	/// </summary>
	public Dictionary<string, string>? Tags { get; set; }
}
