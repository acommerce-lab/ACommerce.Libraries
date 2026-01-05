using Microsoft.AspNetCore.Http;

namespace ACommerce.Marketing.Analytics.Services;

/// <summary>
/// قارئ موافقة التتبع من الـ HTTP Request
/// يستخدم IHttpContextAccessor للوصول إلى الهيدرات
/// </summary>
public class TrackingConsentReader : ITrackingConsentReader
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    /// <summary>
    /// اسم الهيدر المستخدم لنقل حالة موافقة التتبع
    /// </summary>
    public const string TrackingConsentHeader = "X-Tracking-Consent";

    public TrackingConsentReader(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public bool IsTrackingAllowed()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
        {
            // إذا لم يكن هناك HttpContext (مثل background jobs)، نسمح بالتتبع المحلي فقط
            // ولكن نمنع الإرسال لمنصات خارجية
            return false;
        }

        // قراءة الهيدر
        if (httpContext.Request.Headers.TryGetValue(TrackingConsentHeader, out var headerValue))
        {
            // القيمة "1" تعني السماح، أي قيمة أخرى تعني الرفض
            return headerValue.ToString() == "1";
        }

        // إذا لم يوجد الهيدر، نفترض عدم الموافقة (الأكثر أماناً)
        return false;
    }
}
