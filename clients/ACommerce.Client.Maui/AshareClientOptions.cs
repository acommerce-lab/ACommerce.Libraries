namespace ACommerce.Client.Maui;

/// <summary>
/// إعدادات عميل عشير للتطبيق الأمامي.
/// </summary>
public class AshareClientOptions
{
    /// <summary>عنوان الخادم الأساسي (مثل: https://api.ashare.app)</summary>
    public required string BaseAddress { get; set; }

    /// <summary>معرّف طرف العميل في القيود المحاسبية (Client:device-xyz)</summary>
    public string? ClientPartyId { get; set; }

    /// <summary>معرّف طرف الخادم في القيود (Server:api.ashare.app)</summary>
    public string? ServerPartyId { get; set; }

    /// <summary>مهلة HTTP الافتراضية</summary>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>توكن مصادقة ثابت (اختياري - للاختبار)</summary>
    public string? BearerToken { get; set; }
}
