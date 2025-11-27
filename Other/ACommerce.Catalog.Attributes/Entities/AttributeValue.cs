using System.ComponentModel.DataAnnotations.Schema;
using ACommerce.SharedKernel.Abstractions.Entities;

namespace ACommerce.Catalog.Attributes.Entities;

/// <summary>
/// قيمة محددة للخاصية (مثال: "أحمر" للون، "كبير" للحجم)
/// </summary>
public class AttributeValue : IBaseEntity
{
	public Guid Id { get; set; }
	public DateTime CreatedAt { get; set; }
	public DateTime? UpdatedAt { get; set; }
	public bool IsDeleted { get; set; }

	/// <summary>
	/// معرف الخاصية الأم
	/// </summary>
	public Guid AttributeDefinitionId { get; set; }
	public AttributeDefinition? AttributeDefinition { get; set; }

	/// <summary>
	/// القيمة الفعلية
	/// مثال: "أحمر", "كبير", "قطن"
	/// </summary>
	public required string Value { get; set; }

	/// <summary>
	/// اسم العرض (مختلف عن القيمة إن لزم)
	/// مثال: Value="red", DisplayName="أحمر"
	/// </summary>
	public string? DisplayName { get; set; }

	/// <summary>
	/// رمز مختصر (اختياري)
	/// مثال: "RD" للأحمر
	/// </summary>
	public string? Code { get; set; }

	/// <summary>
	/// الوصف
	/// </summary>
	public string? Description { get; set; }

	/// <summary>
	/// لون (إذا كانت الخاصية لون)
	/// مثال: "#FF0000"
	/// </summary>
	public string? ColorHex { get; set; }

	/// <summary>
	/// صورة (URL أو Path)
	/// </summary>
	public string? ImageUrl { get; set; }

	/// <summary>
	/// ترتيب العرض
	/// </summary>
	public int SortOrder { get; set; }

	/// <summary>
	/// هل القيمة نشطة؟
	/// </summary>
	public bool IsActive { get; set; } = true;

	/// <summary>
	/// معلومات إضافية
	/// </summary>
	[NotMapped]
	public Dictionary<string, string> Metadata { get; set; } = new();

	/// <summary>
	/// العلاقات مع قيم أخرى (مثل: الأحمر يتوافق مع القطن)
	/// </summary>
	public List<AttributeValueRelationship> ParentRelationships { get; set; } = new();
	public List<AttributeValueRelationship> ChildRelationships { get; set; } = new();
}
