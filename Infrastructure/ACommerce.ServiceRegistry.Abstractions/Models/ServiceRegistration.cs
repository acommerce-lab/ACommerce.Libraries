namespace ACommerce.ServiceRegistry.Abstractions.Models;

/// <summary>
/// بيانات تسجيل خدمة جديدة
/// </summary>
public sealed class ServiceRegistration
{
	/// <summary>
	/// اسم الخدمة (مطلوب)
	/// </summary>
	public string ServiceName { get; set; } = string.Empty;

	/// <summary>
	/// إصدار الخدمة (مطلوب)
	/// </summary>
	public string Version { get; set; } = "v1";

	/// <summary>
	/// عنوان URL الأساسي (مطلوب)
	/// </summary>
	public string BaseUrl { get; set; } = string.Empty;

	/// <summary>
	/// البيئة (اختياري)
	/// </summary>
	public string Environment { get; set; } = "Development";

	/// <summary>
	/// الوزن للتوزيع (اختياري)
	/// </summary>
	public int Weight { get; set; } = 100;

	/// <summary>
	/// Tags إضافية (اختياري)
	/// </summary>
	public Dictionary<string, string>? Tags { get; set; }

	/// <summary>
	/// هل تريد تفعيل Health Checks؟ (افتراضياً: نعم)
	/// </summary>
	public bool EnableHealthCheck { get; set; } = true;

	/// <summary>
	/// مسار Health Check Endpoint (افتراضياً: /health)
	/// </summary>
	public string HealthCheckPath { get; set; } = "/health";

	/// <summary>
	/// كم ثانية بين كل فحص؟ (افتراضياً: 30 ثانية)
	/// </summary>
	public int HealthCheckIntervalSeconds { get; set; } = 30;
}
