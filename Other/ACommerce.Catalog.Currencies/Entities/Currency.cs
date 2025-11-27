using System.ComponentModel.DataAnnotations.Schema;
using ACommerce.SharedKernel.Abstractions.Entities;

namespace ACommerce.Catalog.Currencies.Entities;

/// <summary>
/// العملة (SAR, USD, EUR, GBP, etc.)
/// نظام مستقل - لا يرث من Unit أو AttributeValue
/// </summary>
public class Currency : IBaseEntity
{
	public Guid Id { get; set; }
	public DateTime CreatedAt { get; set; }
	public DateTime? UpdatedAt { get; set; }
	public bool IsDeleted { get; set; }

	/// <summary>
	/// رمز العملة (ISO 4217)
	/// مثال: "SAR", "USD", "EUR", "GBP"
	/// </summary>
	public required string Code { get; set; }

	/// <summary>
	/// اسم العملة
	/// مثال: "الريال السعودي", "الدولار الأمريكي"
	/// </summary>
	public required string Name { get; set; }

	/// <summary>
	/// الرمز (﷼, $, €, £)
	/// </summary>
	public required string Symbol { get; set; }

	/// <summary>
	/// عدد الخانات العشرية (عادة 2، بعض العملات 0 أو 3)
	/// مثال: الريال السعودي = 2، الدينار البحريني = 3، الين الياباني = 0
	/// </summary>
	public int DecimalPlaces { get; set; } = 2;

	/// <summary>
	/// هل الرمز يأتي قبل المبلغ أم بعده؟
	/// true: $100, false: 100﷼
	/// </summary>
	public bool SymbolBeforeAmount { get; set; } = true;

	/// <summary>
	/// فاصل الآلاف (Thousands Separator)
	/// مثال: "," في الإنجليزية، "." في الألمانية
	/// </summary>
	public string ThousandsSeparator { get; set; } = ",";

	/// <summary>
	/// فاصل العشرات (Decimal Separator)
	/// مثال: "." في الإنجليزية، "," في الأوروبية
	/// </summary>
	public string DecimalSeparator { get; set; } = ".";

	/// <summary>
	/// هل هي العملة الافتراضية للنظام؟
	/// </summary>
	public bool IsBaseCurrency { get; set; }

	/// <summary>
	/// هل العملة نشطة ومتاحة؟
	/// </summary>
	public bool IsActive { get; set; } = true;

	/// <summary>
	/// ترتيب العرض
	/// </summary>
	public int SortOrder { get; set; }

	/// <summary>
	/// أسماء الدول التي تستخدم هذه العملة
	/// </summary>
	public List<string> Countries { get; set; } = new();

	/// <summary>
	/// معلومات إضافية
	/// </summary>
	[NotMapped]
	public Dictionary<string, string> Metadata { get; set; } = new();

	/// <summary>
	/// أسعار الصرف من هذه العملة
	/// </summary>
	public List<ExchangeRate> ExchangeRatesFrom { get; set; } = new();

	/// <summary>
	/// أسعار الصرف إلى هذه العملة
	/// </summary>
	public List<ExchangeRate> ExchangeRatesTo { get; set; } = new();
}
