using ACommerce.SharedKernel.Abstractions.Entities;
using System.ComponentModel.DataAnnotations.Schema;

namespace ACommerce.Reviews.Entities;

/// <summary>
/// تقييم - قابل للربط بأي كيان
/// </summary>
public class Review : IBaseEntity
{
	public Guid Id { get; set; }
	public DateTime CreatedAt { get; set; }
	public DateTime? UpdatedAt { get; set; }
	public bool IsDeleted { get; set; }

	/// <summary>
	/// نوع الكيان (Product, Vendor, Order, etc)
	/// </summary>
	public required string EntityType { get; set; }

	/// <summary>
	/// معرف الكيان
	/// </summary>
	public required Guid EntityId { get; set; }

	/// <summary>
	/// معرف المستخدم
	/// </summary>
	public required string UserId { get; set; }

	/// <summary>
	/// التقييم (1-5)
	/// </summary>
	public required int Rating { get; set; }

	/// <summary>
	/// العنوان
	/// </summary>
	public string? Title { get; set; }

	/// <summary>
	/// التعليق
	/// </summary>
	public string? Comment { get; set; }

	/// <summary>
	/// الإيجابيات
	/// </summary>
	public List<string> Pros { get; set; } = new();

	/// <summary>
	/// السلبيات
	/// </summary>
	public List<string> Cons { get; set; } = new();

	/// <summary>
	/// الصور المرفقة
	/// </summary>
	public List<string> Images { get; set; } = new();

	/// <summary>
	/// مشتري موثق
	/// </summary>
	public bool IsVerifiedPurchase { get; set; }

	/// <summary>
	/// معتمد من المشرف
	/// </summary>
	public bool IsApproved { get; set; }

	/// <summary>
	/// عدد التصويتات المفيدة
	/// </summary>
	public int HelpfulCount { get; set; }

	/// <summary>
	/// رد البائع
	/// </summary>
	public string? VendorResponse { get; set; }

	/// <summary>
	/// تاريخ رد البائع
	/// </summary>
	public DateTime? VendorResponseAt { get; set; }

	/// <summary>
	/// بيانات إضافية
	/// </summary>
	[NotMapped] public Dictionary<string, string> Metadata { get; set; } = new();
}
