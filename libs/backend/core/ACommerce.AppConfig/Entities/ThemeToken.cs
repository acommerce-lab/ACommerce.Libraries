using ACommerce.AppConfig.Enums;
using ACommerce.SharedKernel.Abstractions.Entities;

namespace ACommerce.AppConfig.Entities;

/// <summary>
/// متغير ثيم (CSS variable / MAUI color resource) قابل للتعديل من الخادم.
/// مفتاح فريد لكل (Key, Mode).
/// مثال: (primary, Light) = "#345454"، (primary, Dark) = "#5A8585".
/// </summary>
public class ThemeToken : IBaseEntity
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }

    /// <summary>
    /// اسم الـ token بدون البادئة (مثلاً "primary" ينتج --ashare-primary على الويب،
    /// أو يُسند إلى Primary في MAUI Colors.xaml).
    /// </summary>
    public required string Key { get; set; }

    /// <summary>وضع الثيم (Light / Dark).</summary>
    public ThemeMode Mode { get; set; } = ThemeMode.Light;

    /// <summary>القيمة (لون HEX، أو أي قيمة CSS صالحة).</summary>
    public required string Value { get; set; }

    /// <summary>هل الـ token نشط؟ التعطيل يعيد العميل للقيمة الافتراضية من الكود/CSS.</summary>
    public bool IsActive { get; set; } = true;
}
