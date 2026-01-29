using Ashare.Shared.Services;

namespace Ashare.App.Services;

/// <summary>
/// خدمة إدارة موافقة التتبع للتطبيق
/// تستخدم ATT على iOS و Preferences للتخزين المحلي
/// </summary>
public class TrackingConsentService : ITrackingConsentService
{
    private const string ConsentStatusKey = "tracking_consent_status";
    private const string ConsentRequestedKey = "tracking_consent_requested";

    private TrackingConsentStatus? _consentStatus;
    private bool _isInitialized;

    public event Action<TrackingConsentStatus>? ConsentStatusChanged;

    public TrackingConsentService()
    {
        // لا نقوم بتحميل الحالة في constructor لتجنب crash على iOS
        // سيتم تحميلها عند أول طلب للحالة
    }

    public TrackingConsentStatus ConsentStatus
    {
        get
        {
            EnsureInitialized();
            return _consentStatus ?? TrackingConsentStatus.NotDetermined;
        }
    }

    public bool HasRequestedConsent => Preferences.Get(ConsentRequestedKey, false);

    public bool IsTrackingAllowed
    {
        get
        {
            EnsureInitialized();
            return _consentStatus == TrackingConsentStatus.Authorized;
        }
    }

    /// <summary>
    /// تهيئة الخدمة بشكل آمن (تُستدعى عند أول طلب للحالة)
    /// </summary>
    private void EnsureInitialized()
    {
        if (_isInitialized)
            return;

        try
        {
            LoadSavedStatus();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[TrackingConsent] Error during initialization: {ex.Message}");
            _consentStatus = TrackingConsentStatus.NotDetermined;
        }
        finally
        {
            _isInitialized = true;
        }
    }

    public async Task<TrackingConsentStatus> RequestConsentAsync()
    {
        try
        {
#if IOS
            // على iOS، نستخدم ATT Framework
            _consentStatus = await RequestIOSTrackingAuthorizationAsync();
#elif ANDROID
            // على Android، نوافق افتراضياً (لا يوجد ATT)
            // يمكن إضافة نافذة موافقة مخصصة إذا أردت
            _consentStatus = TrackingConsentStatus.Authorized;
#else
            // على المنصات الأخرى، نوافق افتراضياً
            _consentStatus = TrackingConsentStatus.Authorized;
#endif

            // حفظ الحالة
            SaveStatus(_consentStatus ?? TrackingConsentStatus.NotDetermined);
            Preferences.Set(ConsentRequestedKey, true);

            // إطلاق الحدث
            var currentStatus = _consentStatus ?? TrackingConsentStatus.NotDetermined;
            ConsentStatusChanged?.Invoke(currentStatus);

            Console.WriteLine($"[TrackingConsent] Status: {currentStatus}");
            return currentStatus;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[TrackingConsent] Error requesting consent: {ex.Message}");
            return TrackingConsentStatus.Denied;
        }
    }

    public string GetTrackingHeaderValue()
    {
        // إرجاع قيمة بسيطة: "1" للسماح، "0" للرفض
        return IsTrackingAllowed ? "1" : "0";
    }

#if IOS
    private async Task<TrackingConsentStatus> RequestIOSTrackingAuthorizationAsync()
    {
        try
        {
            // التحقق من الإصدار (ATT متوفر في iOS 14+)
            if (!UIKit.UIDevice.CurrentDevice.CheckSystemVersion(14, 0))
            {
                // iOS أقل من 14 - نعتبره موافقة تلقائية
                return TrackingConsentStatus.Authorized;
            }

            var status = AppTrackingTransparency.ATTrackingManager.TrackingAuthorizationStatus;

            // إذا لم يتم تحديد الحالة بعد، نطلب الموافقة
            if (status == AppTrackingTransparency.ATTrackingManagerAuthorizationStatus.NotDetermined)
            {
                var tcs = new TaskCompletionSource<TrackingConsentStatus>();

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    AppTrackingTransparency.ATTrackingManager.RequestTrackingAuthorization(authStatus =>
                    {
                        var result = MapIOSStatus(authStatus);
                        tcs.TrySetResult(result);
                    });
                });

                return await tcs.Task;
            }

            return MapIOSStatus(status);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[TrackingConsent iOS] Error: {ex.Message}");
            return TrackingConsentStatus.Denied;
        }
    }

    private static TrackingConsentStatus MapIOSStatus(AppTrackingTransparency.ATTrackingManagerAuthorizationStatus status)
    {
        return status switch
        {
            AppTrackingTransparency.ATTrackingManagerAuthorizationStatus.NotDetermined => TrackingConsentStatus.NotDetermined,
            AppTrackingTransparency.ATTrackingManagerAuthorizationStatus.Restricted => TrackingConsentStatus.Restricted,
            AppTrackingTransparency.ATTrackingManagerAuthorizationStatus.Denied => TrackingConsentStatus.Denied,
            AppTrackingTransparency.ATTrackingManagerAuthorizationStatus.Authorized => TrackingConsentStatus.Authorized,
            _ => TrackingConsentStatus.Denied
        };
    }
#endif

    private void LoadSavedStatus()
    {
        var savedStatus = Preferences.Get(ConsentStatusKey, (int)TrackingConsentStatus.NotDetermined);
        _consentStatus = (TrackingConsentStatus)savedStatus;

#if IOS
        // على iOS، نتحقق من الحالة الفعلية من النظام
        if (UIKit.UIDevice.CurrentDevice.CheckSystemVersion(14, 0))
        {
            var systemStatus = AppTrackingTransparency.ATTrackingManager.TrackingAuthorizationStatus;
            _consentStatus = MapIOSStatus(systemStatus);
        }
#endif
    }

    private void SaveStatus(TrackingConsentStatus status)
    {
        Preferences.Set(ConsentStatusKey, (int)status);
    }
}
