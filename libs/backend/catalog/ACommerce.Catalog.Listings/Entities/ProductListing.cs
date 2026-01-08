using System.ComponentModel.DataAnnotations.Schema;
using ACommerce.Catalog.Listings.Enums;
using ACommerce.SharedKernel.Abstractions.Entities;

namespace ACommerce.Catalog.Listings.Entities;

/// <summary>
/// عرض المنتج - ربط البائع بالمنتج
/// هذا هو جوهر Multi-Vendor: البائع "يعرض" المنتج بسعره ومخزونه
/// </summary>
public class ProductListing : IBaseEntity
{
	public Guid Id { get; set; }
	public DateTime CreatedAt { get; set; }
	public DateTime? UpdatedAt { get; set; }
	public bool IsDeleted { get; set; }

	/// <summary>
	/// معرف البائع
	/// </summary>
	public Guid VendorId { get; set; }

	/// <summary>
	/// معرف المنتج (من Catalog.Products)
	/// </summary>
	public Guid ProductId { get; set; }

	/// <summary>
	/// معرف الفئة
	/// </summary>
	public Guid? CategoryId { get; set; }

	/// <summary>
	/// عنوان العرض
	/// </summary>
	public string Title { get; set; } = string.Empty;

	/// <summary>
	/// وصف العرض
	/// </summary>
	public string? Description { get; set; }

	/// <summary>
	/// SKU خاص بالبائع (اختياري)
	/// </summary>
	public string? VendorSku { get; set; }

	/// <summary>
	/// حالة العرض
	/// </summary>
	public ListingStatus Status { get; set; } = ListingStatus.Draft;

	/// <summary>
	/// السعر الأساسي
	/// </summary>
	public required decimal Price { get; set; }

	/// <summary>
	/// سعر المقارنة (قبل الخصم)
	/// </summary>
	public decimal? CompareAtPrice { get; set; }

	/// <summary>
	/// تكلفة المنتج (للبائع)
	/// </summary>
	public decimal? Cost { get; set; }

	/// <summary>
	/// العملة
	/// </summary>
	public Guid? CurrencyId { get; set; }

	/// <summary>
	/// الكمية المتوفرة
	/// </summary>
	public int QuantityAvailable { get; set; }

	/// <summary>
	/// الكمية المحجوزة
	/// </summary>
	public int QuantityReserved { get; set; }

	/// <summary>
	/// حد التنبيه للمخزون
	/// </summary>
	public int? LowStockThreshold { get; set; }

	/// <summary>
	/// وقت التحضير (بالأيام)
	/// </summary>
	public int? ProcessingTime { get; set; }

	/// <summary>
	/// شروط خاصة بالبائع
	/// </summary>
	public string? VendorNotes { get; set; }

	/// <summary>
	/// تاريخ بداية العرض
	/// </summary>
	public DateTime? StartsAt { get; set; }

	/// <summary>
	/// تاريخ نهاية العرض
	/// </summary>
	public DateTime? EndsAt { get; set; }

	/// <summary>
	/// نشط
	/// </summary>
	public bool IsActive { get; set; } = true;

	/// <summary>
	/// عرض مميز
	/// </summary>
	public bool IsFeatured { get; set; }

	/// <summary>
	/// عرض جديد (يُحسب تلقائياً أو يُعين يدوياً)
	/// </summary>
	public bool IsNew { get; set; }

	/// <summary>
	/// عدد المبيعات
	/// </summary>
	public int TotalSales { get; set; }

	/// <summary>
	/// عدد المشاهدات
	/// </summary>
	public int ViewCount { get; set; }

	/// <summary>
	/// التقييم
	/// </summary>
	public decimal? Rating { get; set; }

	/// <summary>
	/// عدد التقييمات
	/// </summary>
	public int ReviewCount { get; set; }

	/// <summary>
	/// بيانات إضافية
	/// </summary>
	[NotMapped]
	public Dictionary<string, string> Metadata { get; set; } = new();

	/// <summary>
	/// الصور (JSON array of URLs)
	/// </summary>
	public string? ImagesJson { get; set; }

	/// <summary>
	/// الصورة المميزة
	/// </summary>
	public string? FeaturedImage { get; set; }

	/// <summary>
	/// خط العرض
	/// </summary>
	public double? Latitude { get; set; }

	/// <summary>
	/// خط الطول
	/// </summary>
	public double? Longitude { get; set; }

	/// <summary>
	/// العنوان
	/// </summary>
	public string? Address { get; set; }

	/// <summary>
	/// المدينة
	/// </summary>
	public string? City { get; set; }

	/// <summary>
	/// حالة المنتج (جديد، مستعمل، إلخ)
	/// </summary>
	public string? Condition { get; set; }

	/// <summary>
	/// رمز العملة
	/// </summary>
	public string? Currency { get; set; }

	/// <summary>
	/// نسبة العمولة/العربون المطلوبة للحجز (من 0 إلى 100)
	/// يتم نسخها من باقة المستخدم عند إنشاء العرض
	/// مثال: 3% للسنوي، 5% للتجاري، 100% لليومي
	/// </summary>
	public decimal CommissionPercentage { get; set; }

	/// <summary>
	/// الخصائص الديناميكية (JSON)
	/// </summary>
	public string? AttributesJson { get; set; }
}
