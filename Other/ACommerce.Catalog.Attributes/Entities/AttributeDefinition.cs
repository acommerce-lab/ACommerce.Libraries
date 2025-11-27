using System.ComponentModel.DataAnnotations.Schema;
using ACommerce.Catalog.Attributes.Enums;
using ACommerce.SharedKernel.Abstractions.Entities;

namespace ACommerce.Catalog.Attributes.Entities;

/// <summary>
/// تعريف الخاصية (مثل: اللون، الحجم، المادة، العلامة التجارية)
/// نظام مستقل تماماً - بدون وراثة أو علاقات دورية
/// </summary>
public class AttributeDefinition : IBaseEntity
{
	public Guid Id { get; set; }
	public DateTime CreatedAt { get; set; }
	public DateTime? UpdatedAt { get; set; }
	public bool IsDeleted { get; set; }

	/// <summary>
	/// اسم الخاصية
	/// مثال: "اللون", "الحجم", "المادة", "العلامة التجارية"
	/// </summary>
	public required string Name { get; set; }

	/// <summary>
	/// الرمز الفريد
	/// مثال: "color", "size", "material", "brand"
	/// </summary>
	public required string Code { get; set; }

	/// <summary>
	/// نوع الخاصية (Text, Number, SingleSelect, MultiSelect, Boolean, Date)
	/// </summary>
	public AttributeType Type { get; set; }

	/// <summary>
	/// الوصف
	/// </summary>
	public string? Description { get; set; }

	/// <summary>
	/// هل الخاصية إلزامية؟
	/// </summary>
	public bool IsRequired { get; set; }

	/// <summary>
	/// هل تظهر في البحث/الفلاتر؟
	/// </summary>
	public bool IsFilterable { get; set; }

	/// <summary>
	/// هل تظهر في قائمة المنتجات؟
	/// </summary>
	public bool IsVisibleInList { get; set; } = true;

	/// <summary>
	/// هل تظهر في تفاصيل المنتج؟
	/// </summary>
	public bool IsVisibleInDetail { get; set; } = true;

	/// <summary>
	/// ترتيب العرض
	/// </summary>
	public int SortOrder { get; set; }

	/// <summary>
	/// قواعد التحقق (JSON Schema)
	/// مثال: { "min": 0, "max": 100, "pattern": "^[A-Z]+$" }
	/// </summary>
	public string? ValidationRules { get; set; }

	/// <summary>
	/// القيمة الافتراضية
	/// </summary>
	public string? DefaultValue { get; set; }

	/// <summary>
	/// معلومات إضافية
	/// </summary>
	[NotMapped]
	public Dictionary<string, string> Metadata { get; set; } = new();

	/// <summary>
	/// القيم الممكنة لهذه الخاصية (للأنواع Select)
	/// </summary>
	public List<AttributeValue> Values { get; set; } = new();
}
