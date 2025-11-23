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
	public Dictionary<string, string> Metadata { get; set; } = new();
}
