using ACommerce.SharedKernel.Abstractions.Entities;

namespace ACommerce.AppConfig.Entities;

/// <summary>
/// مفتاح نص واجهة (UI string key) قابل للتجاوز ديناميكياً.
/// النموذج Hybrid: الكود يحوي defaults، DB تحوي فقط المفاتيح المُعدّلة (overrides).
/// مفتاح فريد لكل (Key, Language).
/// </summary>
public class UiString : IBaseEntity
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }

    /// <summary>اسم المفتاح كما يستخدمه الكود (مثلاً AppTagline, Disclaimer).</summary>
    public required string Key { get; set; }

    /// <summary>رمز اللغة (ar, en, ur).</summary>
    public required string Language { get; set; }

    /// <summary>النص المعروض. يدعم HTML بسيط حيث يسمح به الـ binding.</summary>
    public required string Value { get; set; }

    /// <summary>هل المفتاح نشط؟ التعطيل يجعل العميل يعود للـdefault من الكود.</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>تعليق إداري (لمن سيعدّله لاحقاً).</summary>
    public string? Note { get; set; }
}
