using ACommerce.SharedKernel.Abstractions.Entities;
using System.ComponentModel.DataAnnotations.Schema;

namespace ACommerce.Catalog.Units.Entities;

/// <summary>
/// تحويل مباشر بين وحدتين (للتحويلات الشائعة المحسنة)
/// </summary>
public class UnitConversion : IBaseEntity
{
	public Guid Id { get; set; }
	public DateTime CreatedAt { get; set; }
	public DateTime? UpdatedAt { get; set; }
	public bool IsDeleted { get; set; }

	/// <summary>
	/// الوحدة المصدر
	/// </summary>
	public Guid FromUnitId { get; set; }
	public Unit? FromUnit { get; set; }

	/// <summary>
	/// الوحدة الهدف
	/// </summary>
	public Guid ToUnitId { get; set; }
	public Unit? ToUnit { get; set; }

	/// <summary>
	/// معامل التحويل المباشر
	/// مثال: 1 Kilogram = 2.20462 Pound → ConversionFactor = 2.20462
	/// </summary>
	public decimal ConversionFactor { get; set; }

	/// <summary>
	/// معادلة التحويل (للتحويلات المعقدة)
	/// مثال: "value * 2.20462" أو "(value - 32) * 5/9"
	/// </summary>
	public string? Formula { get; set; }

	/// <summary>
	/// الأولوية (في حال وجود أكثر من طريق تحويل)
	/// </summary>
	public int Priority { get; set; }

    /// <summary>
    /// معلومات إضافية
    /// </summary>
    [NotMapped] public Dictionary<string, string> Metadata { get; set; } = new();
}
