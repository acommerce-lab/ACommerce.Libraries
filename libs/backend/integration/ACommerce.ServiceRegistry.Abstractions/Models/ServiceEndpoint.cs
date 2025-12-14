using System.ComponentModel.DataAnnotations.Schema;

namespace ACommerce.ServiceRegistry.Abstractions.Models;

/// <summary>
/// معلومات نقطة النهاية للخدمة (Service Endpoint)
/// </summary>
public sealed class ServiceEndpoint
{
	/// <summary>
	/// معرف فريد لنقطة النهاية
	/// </summary>
	public string Id { get; set; } = Guid.NewGuid().ToString();

	/// <summary>
	/// اسم الخدمة (مثل: Products, Orders, Payments)
	/// </summary>
	public string ServiceName { get; set; } = string.Empty;

	/// <summary>
	/// إصدار الخدمة (مثل: v1, v2)
	/// </summary>
	public string Version { get; set; } = "v1";

	/// <summary>
	/// عنوان URL الأساسي للخدمة
	/// </summary>
	public string BaseUrl { get; set; } = string.Empty;

	/// <summary>
	/// البيئة (Development, Staging, Production)
	/// </summary>
	public string Environment { get; set; } = "Development";

	/// <summary>
	/// الوزن للتوزيع (Load Balancing) - كلما زاد كلما أخذت طلبات أكثر
	/// </summary>
	public int Weight { get; set; } = 100;

    /// <summary>
    /// Tags إضافية للتصنيف
    /// </summary>
    [NotMapped] public Dictionary<string, string> Tags { get; set; } = new();

	/// <summary>
	/// معلومات الصحة الحالية
	/// </summary>
	public ServiceHealth Health { get; set; } = new();

	/// <summary>
	/// وقت آخر تحديث
	/// </summary>
	public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

	/// <summary>
	/// متى تم التسجيل أول مرة
	/// </summary>
	public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;

	/// <summary>
	/// هل الخدمة نشطة حالياً؟
	/// </summary>
	public bool IsActive => Health.Status == HealthStatus.Healthy;
}
