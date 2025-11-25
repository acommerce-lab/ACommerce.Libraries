using ACommerce.SharedKernel.Abstractions.Entities;

namespace ACommerce.Catalog.Currencies.Entities;

/// <summary>
/// سعر الصرف بين عملتين
/// </summary>
public class ExchangeRate : IBaseEntity
{
	public Guid Id { get; set; }
	public DateTime CreatedAt { get; set; }
	public DateTime? UpdatedAt { get; set; }
	public bool IsDeleted { get; set; }

	/// <summary>
	/// العملة المصدر (From)
	/// </summary>
	public Guid FromCurrencyId { get; set; }
	public Currency? FromCurrency { get; set; }

	/// <summary>
	/// العملة الهدف (To)
	/// </summary>
	public Guid ToCurrencyId { get; set; }
	public Currency? ToCurrency { get; set; }

	/// <summary>
	/// سعر الصرف
	/// مثال: 1 USD = 3.75 SAR → Rate = 3.75
	/// </summary>
	public decimal Rate { get; set; }

	/// <summary>
	/// تاريخ بدء السريان
	/// </summary>
	public DateTime EffectiveFrom { get; set; }

	/// <summary>
	/// تاريخ انتهاء السريان (null = بدون انتهاء)
	/// </summary>
	public DateTime? EffectiveTo { get; set; }

	/// <summary>
	/// المصدر (API، يدوي، تلقائي)
	/// </summary>
	public string Source { get; set; } = "Manual";

	/// <summary>
	/// هل السعر نشط؟
	/// </summary>
	public bool IsActive { get; set; } = true;

	/// <summary>
	/// معلومات إضافية
	/// </summary>
	public Dictionary<string, string> Metadata { get; set; } = new();
}

