namespace ACommerce.ServiceRegistry.Abstractions.Models;

/// <summary>
/// معلومات صحة الخدمة (Health Status)
/// </summary>
public sealed class ServiceHealth
{
	/// <summary>
	/// حالة الخدمة الحالية
	/// </summary>
	public HealthStatus Status { get; set; } = HealthStatus.Unknown;

	/// <summary>
	/// وقت آخر فحص
	/// </summary>
	public DateTime LastChecked { get; set; } = DateTime.UtcNow;

	/// <summary>
	/// وقت الاستجابة بالميللي ثانية
	/// </summary>
	public long ResponseTimeMs { get; set; }

	/// <summary>
	/// رسالة إضافية (في حالة خطأ مثلاً)
	/// </summary>
	public string? Message { get; set; }

	/// <summary>
	/// عدد المحاولات الفاشلة المتتالية
	/// </summary>
	public int ConsecutiveFailures { get; set; }

	/// <summary>
	/// معلومات إضافية عن الصحة
	/// </summary>
	public Dictionary<string, object> Details { get; set; } = new();
}

/// <summary>
/// حالات صحة الخدمة
/// </summary>
public enum HealthStatus
{
	/// <summary>
	/// غير معروف - لم يتم الفحص بعد
	/// </summary>
	Unknown = 0,

	/// <summary>
	/// سليمة - تعمل بشكل طبيعي
	/// </summary>
	Healthy = 1,

	/// <summary>
	/// متدهورة - تعمل لكن ببطء أو بمشاكل
	/// </summary>
	Degraded = 2,

	/// <summary>
	/// معطلة - لا تستجيب
	/// </summary>
	Unhealthy = 3
}
