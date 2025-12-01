using ACommerce.SharedKernel.Abstractions.Entities;

namespace ACommerce.Catalog.Attributes.Entities;

/// <summary>
/// جدول ربط الفئات بالخصائص
/// يحدد أي خصائص تظهر لكل فئة
/// </summary>
public class CategoryAttributeMapping : IBaseEntity
{
	public Guid Id { get; set; }
	public DateTime CreatedAt { get; set; }
	public DateTime? UpdatedAt { get; set; }
	public bool IsDeleted { get; set; }

	/// <summary>
	/// معرّف الفئة
	/// </summary>
	public Guid CategoryId { get; set; }

	/// <summary>
	/// معرّف الخاصية
	/// </summary>
	public Guid AttributeDefinitionId { get; set; }

	/// <summary>
	/// الخاصية المرتبطة
	/// </summary>
	public AttributeDefinition AttributeDefinition { get; set; } = null!;

	/// <summary>
	/// ترتيب العرض ضمن الفئة
	/// </summary>
	public int SortOrder { get; set; }

	/// <summary>
	/// هل الخاصية مطلوبة في هذه الفئة؟
	/// (يتجاوز القيمة الافتراضية في AttributeDefinition)
	/// </summary>
	public bool? IsRequiredOverride { get; set; }

	/// <summary>
	/// هل الخاصية مفعّلة في هذه الفئة؟
	/// </summary>
	public bool IsActive { get; set; } = true;
}
