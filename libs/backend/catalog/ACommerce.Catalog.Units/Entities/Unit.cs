using System.ComponentModel.DataAnnotations.Schema;
using ACommerce.SharedKernel.Abstractions.Entities;
using System.ComponentModel.DataAnnotations.Schema;

namespace ACommerce.Catalog.Units.Entities;

/// <summary>
/// الوحدة (وحدة قياس مثل kg, meter, liter)
/// نظام مستقل - لا يرث من AttributeDefinition أو AttributeValue
/// </summary>
public class Unit : IBaseEntity
{
	public Guid Id { get; set; }
	public DateTime CreatedAt { get; set; }
	public DateTime? UpdatedAt { get; set; }
	public bool IsDeleted { get; set; }

	/// <summary>
	/// اسم الوحدة
	/// مثال: "كيلوجرام", "جرام", "متر", "سنتيمتر"
	/// </summary>
	public required string Name { get; set; }

	/// <summary>
	/// الرمز المختصر
	/// مثال: "kg", "g", "m", "cm", "lb", "oz"
	/// </summary>
	public required string Symbol { get; set; }

	/// <summary>
	/// معرف فئة الوحدة
	/// </summary>
	public Guid UnitCategoryId { get; set; }
	public UnitCategory? UnitCategory { get; set; }

	/// <summary>
	/// معرف نظام القياس
	/// </summary>
	public Guid MeasurementSystemId { get; set; }
	public MeasurementSystem? MeasurementSystem { get; set; }

	/// <summary>
	/// معامل التحويل إلى الوحدة الأساسية
	/// مثال:
	/// - 1 Kilogram = 1000 Gram → ConversionToBase = 1000
	/// - 1 Gram = 1 Gram → ConversionToBase = 1
	/// - 1 Pound = 453.592 Gram → ConversionToBase = 453.592
	/// </summary>
	public decimal ConversionToBase { get; set; } = 1;

	/// <summary>
	/// معادلة التحويل (للتحويلات المعقدة مثل الحرارة)
	/// مثال: Celsius to Kelvin = value + 273.15
	/// مثال: Fahrenheit to Celsius = (value - 32) * 5/9
	/// Format: JavaScript expression or Formula string
	/// </summary>
	public string? ConversionFormula { get; set; }

	/// <summary>
	/// عدد الخانات العشرية المطلوبة للدقة
	/// </summary>
	public int DecimalPlaces { get; set; } = 2;

	/// <summary>
	/// هل الوحدة قياسية (Standard) أم مخصصة (Custom)?
	/// </summary>
	public bool IsStandard { get; set; } = true;

	/// <summary>
	/// هل الوحدة نشطة؟
	/// </summary>
	public bool IsActive { get; set; } = true;

	/// <summary>
	/// ترتيب العرض
	/// </summary>
	public int SortOrder { get; set; }

	/// <summary>
	/// الوصف
	/// </summary>
	public string? Description { get; set; }

	/// <summary>
	/// معلومات إضافية
	/// </summary>
	[NotMapped]
	public Dictionary<string, string> Metadata { get; set; } = new();

	/// <summary>
	/// التحويلات المباشرة المعرفة (اختياري)
	/// </summary>
	public List<UnitConversion> ConversionsFrom { get; set; } = new();
	public List<UnitConversion> ConversionsTo { get; set; } = new();
}
