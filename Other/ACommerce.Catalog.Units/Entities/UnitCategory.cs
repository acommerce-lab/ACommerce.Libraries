using ACommerce.SharedKernel.Abstractions.Entities;

namespace ACommerce.Catalog.Units.Entities;

/// <summary>
/// فئة الوحدات (الوزن، الطول، الحجم، المساحة، الحرارة، إلخ)
/// نظام مستقل - لا يرث من AttributeDefinition
/// </summary>
public class UnitCategory : IBaseEntity
{
	public Guid Id { get; set; }
	public DateTime CreatedAt { get; set; }
	public DateTime? UpdatedAt { get; set; }
	public bool IsDeleted { get; set; }

	/// <summary>
	/// اسم الفئة
	/// مثال: "الوزن", "الطول", "الحجم", "المساحة"
	/// </summary>
	public required string Name { get; set; }

	/// <summary>
	/// رمز الفئة
	/// مثال: "weight", "length", "volume", "area", "temperature"
	/// </summary>
	public required string Code { get; set; }

	/// <summary>
	/// الوصف
	/// </summary>
	public string? Description { get; set; }

	/// <summary>
	/// معرف الوحدة الأساسية في هذه الفئة
	/// مثال: Gram للوزن، Meter للطول
	/// </summary>
	public Guid? BaseUnitId { get; set; }
	public Unit? BaseUnit { get; set; }

	/// <summary>
	/// ترتيب العرض
	/// </summary>
	public int SortOrder { get; set; }

	/// <summary>
	/// معلومات إضافية
	/// </summary>
	public Dictionary<string, string> Metadata { get; set; } = new();

	/// <summary>
	/// الوحدات في هذه الفئة
	/// </summary>
	public List<Unit> Units { get; set; } = new();
}
