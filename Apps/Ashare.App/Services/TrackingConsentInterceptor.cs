using Ashare.Shared.Services;

namespace Ashare.App.Services;

/// <summary>
/// HTTP Interceptor لإضافة هيدر موافقة التتبع مع كل طلب
/// الباك إند يقرأ هذا الهيدر لتحديد هل يتتبع المستخدم أم لا
/// </summary>
public sealed class TrackingConsentInterceptor : DelegatingHandler
{
    private readonly ITrackingConsentService _trackingConsentService;

    /// <summary>
    /// اسم الهيدر المستخدم لنقل حالة موافقة التتبع
    /// </summary>
    public const string TrackingConsentHeader = "X-Tracking-Consent";

    public TrackingConsentInterceptor(ITrackingConsentService trackingConsentService)
    {
        _trackingConsentService = trackingConsentService;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        // إضافة هيدر موافقة التتبع
        // القيمة "1" تعني السماح بالتتبع، "0" تعني الرفض
        var consentValue = _trackingConsentService.GetTrackingHeaderValue();
        request.Headers.TryAddWithoutValidation(TrackingConsentHeader, consentValue);

        return await base.SendAsync(request, cancellationToken);
    }
}
