using ACommerce.Profiles.Enums;
using ACommerce.SharedKernel.Abstractions.Entities;

namespace ACommerce.Profiles.Entities;

/// <summary>
/// البروفايل - ملف تعريفي للمستخدم
/// </summary>
public class Profile : IBaseEntity
{
	public Guid Id { get; set; }
	public DateTime CreatedAt { get; set; }
	public DateTime? UpdatedAt { get; set; }
	public bool IsDeleted { get; set; }

	/// <summary>
	/// معرف المستخدم (من Authentication.Users)
	/// </summary>
	public required string UserId { get; set; }

	/// <summary>
	/// نوع البروفايل
	/// </summary>
	public ProfileType Type { get; set; }

	/// <summary>
	/// الاسم الكامل
	/// </summary>
	public string? FullName { get; set; }

	/// <summary>
	/// الاسم التجاري (للبائعين)
	/// </summary>
	public string? BusinessName { get; set; }

	/// <summary>
	/// رقم الهاتف
	/// </summary>
	public string? PhoneNumber { get; set; }

	/// <summary>
	/// البريد الإلكتروني
	/// </summary>
	public string? Email { get; set; }

	/// <summary>
	/// الصورة الشخصية / اللوجو
	/// </summary>
	public string? Avatar { get; set; }

	/// <summary>
	/// العنوان
	/// </summary>
	public string? Address { get; set; }

	/// <summary>
	/// المدينة
	/// </summary>
	public string? City { get; set; }

	/// <summary>
	/// الدولة
	/// </summary>
	public string? Country { get; set; }

	/// <summary>
	/// الرمز البريدي
	/// </summary>
	public string? PostalCode { get; set; }

	/// <summary>
	/// الإحداثيات الجغرافية
	/// </summary>
	public string? Coordinates { get; set; }

	/// <summary>
	/// نشط
	/// </summary>
	public bool IsActive { get; set; } = true;

	/// <summary>
	/// موثق / معتمد (للبائعين)
	/// </summary>
	public bool IsVerified { get; set; }

	/// <summary>
	/// تاريخ التوثيق
	/// </summary>
	public DateTime? VerifiedAt { get; set; }

	/// <summary>
	/// بيانات إضافية مرنة (JSON)
	/// </summary>
	public Dictionary<string, string> Metadata { get; set; } = new();
}
