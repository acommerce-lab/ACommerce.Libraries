using System.ComponentModel.DataAnnotations.Schema;
using ACommerce.SharedKernel.Abstractions.Entities;
using System.ComponentModel.DataAnnotations.Schema;

namespace ACommerce.Localization.Entities;

/// <summary>
/// الترجمة - قابلة للربط بأي كيان وأي حقل
/// </summary>
public class Translation : IBaseEntity
{
	public Guid Id { get; set; }
	public DateTime CreatedAt { get; set; }
	public DateTime? UpdatedAt { get; set; }
	public bool IsDeleted { get; set; }

	/// <summary>
	/// نوع الكيان (Product, Category, Vendor, etc)
	/// </summary>
	public required string EntityType { get; set; }

	/// <summary>
	/// معرف الكيان
	/// </summary>
	public required Guid EntityId { get; set; }

	/// <summary>
	/// اسم الحقل (Name, Description, etc)
	/// </summary>
	public required string FieldName { get; set; }

	/// <summary>
	/// اللغة (ar, en, fr, etc)
	/// </summary>
	public required string Language { get; set; }

	/// <summary>
	/// النص المترجم
	/// </summary>
	public required string TranslatedText { get; set; }

	/// <summary>
	/// مترجم موثق (من مترجم محترف أم آلي)
	/// </summary>
	public bool IsVerified { get; set; }

	/// <summary>
	/// معرف المترجم
	/// </summary>
	public string? TranslatorId { get; set; }

	/// <summary>
	/// بيانات إضافية
	/// </summary>
	[NotMapped]
	public Dictionary<string, string> Metadata { get; set; } = new();
}

/// <summary>
/// اللغة
/// </summary>
public class Language : IBaseEntity
{
	public Guid Id { get; set; }
	public DateTime CreatedAt { get; set; }
	public DateTime? UpdatedAt { get; set; }
	public bool IsDeleted { get; set; }

	/// <summary>
	/// رمز اللغة (ar, en, fr)
	/// </summary>
	public required string Code { get; set; }

	/// <summary>
	/// الاسم الأصلي (العربية, English, Français)
	/// </summary>
	public required string NativeName { get; set; }

	/// <summary>
	/// الاسم بالإنجليزية
	/// </summary>
	public required string EnglishName { get; set; }

	/// <summary>
	/// اتجاه الكتابة (ltr, rtl)
	/// </summary>
	public required string Direction { get; set; }

	/// <summary>
	/// نشطة
	/// </summary>
	public bool IsActive { get; set; }

	/// <summary>
	/// افتراضية
	/// </summary>
	public bool IsDefault { get; set; }

	/// <summary>
	/// ترتيب العرض
	/// </summary>
	public int SortOrder { get; set; }
}
