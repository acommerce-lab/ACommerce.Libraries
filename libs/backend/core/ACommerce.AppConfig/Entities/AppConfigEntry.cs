using ACommerce.AppConfig.Enums;
using ACommerce.SharedKernel.Abstractions.Entities;

namespace ACommerce.AppConfig.Entities;

/// <summary>
/// إدخال إعدادات عام — Key/Value يصلح لأي قيمة (timeouts, URLs, max sizes, …).
/// مثال: search.maxResults = 50، upload.maxImageMB = 10.
/// </summary>
public class AppConfigEntry : IBaseEntity
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }

    /// <summary>المفتاح بالتدوين النقطي (مثلاً search.maxResults).</summary>
    public required string Key { get; set; }

    /// <summary>القيمة كنص — يفسّرها العميل حسب <see cref="ValueType"/>.</summary>
    public required string Value { get; set; }

    /// <summary>نوع القيمة (String, Integer, Boolean, Color, Json, …).</summary>
    public ConfigValueType ValueType { get; set; } = ConfigValueType.String;

    /// <summary>هل القيمة عامة قابلة للعميل، أم سرية على الخادم فقط؟ (مفاتيح API لا تُكشف).</summary>
    public bool IsPublic { get; set; } = true;

    /// <summary>وصف قصير للأدمن.</summary>
    public string? Description { get; set; }
}
