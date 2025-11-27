using ACommerce.SharedKernel.Abstractions.Entities;
using System.ComponentModel.DataAnnotations.Schema;

namespace ACommerce.Catalog.Units.Entities;

/// <summary>
/// نظام القياس (Metric, Imperial, US Customary, etc.)
/// </summary>
public class MeasurementSystem : IBaseEntity
{
	public Guid Id { get; set; }
	public DateTime CreatedAt { get; set; }
	public DateTime? UpdatedAt { get; set; }
	public bool IsDeleted { get; set; }

	/// <summary>
	/// اسم النظام
	/// مثال: "النظام المتري", "النظام الإمبراطوري", "النظام الأمريكي"
	/// </summary>
	public required string Name { get; set; }

	/// <summary>
	/// الرمز
	/// مثال: "metric", "imperial", "us_customary"
	/// </summary>
	public required string Code { get; set; }

	/// <summary>
	/// الوصف
	/// </summary>
	public string? Description { get; set; }

	/// <summary>
	/// هل هو النظام الافتراضي؟
	/// </summary>
	public bool IsDefault { get; set; }

	/// <summary>
	/// الدول/المناطق التي تستخدم هذا النظام
	/// </summary>
	public List<string> Countries { get; set; } = new();

    /// <summary>
    /// معلومات إضافية
    /// </summary>
    [NotMapped] public Dictionary<string, string> Metadata { get; set; } = new();

	/// <summary>
	/// الوحدات في هذا النظام
	/// </summary>
	public List<Unit> Units { get; set; } = new();
}
