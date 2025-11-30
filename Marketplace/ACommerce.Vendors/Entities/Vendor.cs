using System.ComponentModel.DataAnnotations.Schema;
using ACommerce.Vendors.Enums;
using ACommerce.SharedKernel.Abstractions.Entities;
using ACommerce.Vendors.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace ACommerce.Vendors.Entities;

/// <summary>
/// البائع / التاجر
/// </summary>
public class Vendor : IBaseEntity
{
	public Guid Id { get; set; }
	public DateTime CreatedAt { get; set; }
	public DateTime? UpdatedAt { get; set; }
	public bool IsDeleted { get; set; }

	/// <summary>
	/// معرف البروفايل (من Profiles)
	/// </summary>
	public Guid ProfileId { get; set; }

	/// <summary>
	/// الاسم التجاري
	/// </summary>
	public required string StoreName { get; set; }

	/// <summary>
	/// الاسم المختصر (slug)
	/// </summary>
	public required string StoreSlug { get; set; }

	/// <summary>
	/// الوصف
	/// </summary>
	public string? Description { get; set; }

	/// <summary>
	/// اللوجو
	/// </summary>
	public string? Logo { get; set; }

	/// <summary>
	/// صورة الغلاف
	/// </summary>
	public string? BannerImage { get; set; }

	/// <summary>
	/// حالة البائع
	/// </summary>
	public VendorStatus Status { get; set; } = VendorStatus.Pending;

	/// <summary>
	/// نوع العمولة
	/// </summary>
	public CommissionType CommissionType { get; set; } = CommissionType.Percentage;

	/// <summary>
	/// قيمة العمولة (نسبة مئوية أو مبلغ ثابت)
	/// </summary>
	public decimal CommissionValue { get; set; }

	/// <summary>
	/// عمولة إضافية ثابتة (للنوع المختلط)
	/// </summary>
	public decimal? AdditionalFee { get; set; }

	/// <summary>
	/// الحد الأدنى للسحب
	/// </summary>
	public decimal MinimumPayout { get; set; } = 0;

	/// <summary>
	/// التقييم العام
	/// </summary>
	public decimal? Rating { get; set; }

	/// <summary>
	/// عدد المبيعات
	/// </summary>
	public int TotalSales { get; set; }

	/// <summary>
	/// الرصيد المتاح للسحب
	/// </summary>
	public decimal AvailableBalance { get; set; }

	/// <summary>
	/// الرصيد المعلق
	/// </summary>
	public decimal PendingBalance { get; set; }

	/// <summary>
	/// معلومات البنك (مشفرة)
	/// </summary>
	public string? BankInfo { get; set; }

	/// <summary>
	/// معلومات ضريبية
	/// </summary>
	public string? TaxInfo { get; set; }

	/// <summary>
	/// السجل التجاري
	/// </summary>
	public string? CommercialRegister { get; set; }

	/// <summary>
	/// الرقم الضريبي
	/// </summary>
	public string? TaxNumber { get; set; }

	/// <summary>
	/// موافقة على الشروط والأحكام
	/// </summary>
	public bool AgreedToTerms { get; set; }

	/// <summary>
	/// تاريخ الموافقة
	/// </summary>
	public DateTime? AgreedToTermsAt { get; set; }

	/// <summary>
	/// بيانات إضافية
	/// </summary>
	[NotMapped]
	public Dictionary<string, string> Metadata { get; set; } = new();
}
