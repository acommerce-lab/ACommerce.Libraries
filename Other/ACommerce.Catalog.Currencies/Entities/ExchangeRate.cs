using ACommerce.SharedKernel.Abstractions.Entities;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;

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
	/// Gets or sets the type of rate applied to the transaction.
	/// <see langword="string"/>
	/// </summary>
	public string? RateType { get; set; }

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
    /// أولوية سعر الصرف (كلما كان الرقم أقل، كانت الأولوية أعلى)
    /// </summary>
    public int? Priority { get; set; }

    /// <summary>
    /// معلومات إضافية
    /// </summary>
    [NotMapped] public Dictionary<string, string> Metadata { get; set; } = [];
    public decimal InverseRate { get; private set; }

    public void CalculateInverseRate()
    {
        if (Rate == 0)
            throw new InvalidOperationException("Rate cannot be zero when calculating inverse rate.");

        // مثال: 1 USD = 3.75 SAR → inverse = 0.2666...
        var inverseRate = 1 / Rate;

		// ممكن تخزنها في Metadata أو تضيف خاصية جديدة
		InverseRate = inverseRate;
    }
}

