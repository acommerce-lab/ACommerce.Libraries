using System.ComponentModel.DataAnnotations.Schema;
using ACommerce.SharedKernel.Abstractions.Entities;
using System.ComponentModel.DataAnnotations.Schema;

namespace ACommerce.Configuration.Entities;

/// <summary>
/// الإعدادات - مرنة وقابلة للتخصيص
/// </summary>
public class Setting : IBaseEntity
{
	public Guid Id { get; set; }
	public DateTime CreatedAt { get; set; }
	public DateTime? UpdatedAt { get; set; }
	public bool IsDeleted { get; set; }

	/// <summary>
	/// مفتاح الإعداد (Store.Name, Vendor.Commission, Payment.Moyasar.ApiKey)
	/// </summary>
	public required string Key { get; set; }

	/// <summary>
	/// القيمة
	/// </summary>
	public required string Value { get; set; }

	/// <summary>
	/// نطاق الإعداد (Global, Store, Vendor)
	/// </summary>
	public required string Scope { get; set; }

	/// <summary>
	/// معرف النطاق (null للـ Global، StoreId أو VendorId)
	/// </summary>
	public Guid? ScopeId { get; set; }

	/// <summary>
	/// نوع البيانات (String, Int, Bool, Json)
	/// </summary>
	public string DataType { get; set; } = "String";

	/// <summary>
	/// الوصف
	/// </summary>
	public string? Description { get; set; }

	/// <summary>
	/// مشفر؟
	/// </summary>
	public bool IsEncrypted { get; set; }

	/// <summary>
	/// قابل للتعديل من قبل المستخدم؟
	/// </summary>
	public bool IsUserEditable { get; set; } = true;

	/// <summary>
	/// بيانات إضافية
	/// </summary>
	[NotMapped]
	public Dictionary<string, string> Metadata { get; set; } = new();
}
