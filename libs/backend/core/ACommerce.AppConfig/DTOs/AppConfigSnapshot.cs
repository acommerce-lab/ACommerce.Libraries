namespace ACommerce.AppConfig.DTOs;

/// <summary>
/// لقطة كاملة (snapshot) من إعدادات التطبيق — تُسلّم للعميل في طلب واحد.
/// تحتوي على Hash لاكتشاف التغييرات (ETag)، فإن لم يتغير شيء يُرجِع الخادم 304.
/// </summary>
public class AppConfigSnapshot
{
    /// <summary>Hash (SHA256 أو SipHash) للقطة، يُستخدم كـ ETag.</summary>
    public string Version { get; set; } = string.Empty;

    /// <summary>وقت توليد اللقطة (UTC).</summary>
    public DateTime FetchedAt { get; set; }

    /// <summary>اللغة المطلوبة عند الجلب.</summary>
    public string Language { get; set; } = "ar";

    /// <summary>المنصة المُبلَّغ عنها من العميل.</summary>
    public string? Platform { get; set; }

    /// <summary>إصدار العميل.</summary>
    public string? AppVersion { get; set; }

    /// <summary>
    /// خريطة Feature Flag → enabled-for-this-client (بعد تقييم Platform/Version).
    /// مثال: { "payments.noon": true, "booking.enabled": false }
    /// </summary>
    public Dictionary<string, bool> Features { get; set; } = new();

    /// <summary>
    /// مفاتيح النصوص المُعدّلة عبر DB لهذه اللغة. القيم غير الموجودة هنا يستخدم العميل
    /// قيم الـ defaults المضمّنة في الكود.
    /// </summary>
    public Dictionary<string, string> Strings { get; set; } = new();

    /// <summary>
    /// خريطة الـ Theme tokens: Light + Dark.
    /// مثال: { "light": { "primary": "#345454" }, "dark": { "primary": "#5A8585" } }
    /// </summary>
    public Dictionary<string, Dictionary<string, string>> Theme { get; set; } = new()
    {
        ["light"] = new(),
        ["dark"] = new()
    };

    /// <summary>
    /// إعدادات عامة مفتاح/قيمة (timeouts, URLs, limits, …) — يُكشف منها فقط ما IsPublic = true.
    /// </summary>
    public Dictionary<string, string> Config { get; set; } = new();
}
