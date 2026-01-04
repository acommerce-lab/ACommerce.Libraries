using Ashare.App.Services;
using Ashare.Shared.Services;
using Foundation;
using UIKit;

namespace Ashare.App;

[Register("AppDelegate")]
public class AppDelegate : MauiUIApplicationDelegate
{
    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

    public override bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
    {
        var result = base.FinishedLaunching(application, launchOptions);

        // تهيئة خدمة الإسناد
        Task.Run(async () =>
        {
            try
            {
                var attributionService = IPlatformApplication.Current?.Services.GetService<IAttributionCaptureService>();
                if (attributionService != null)
                {
                    await attributionService.InitializeAsync();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Attribution iOS] Init error: {ex.Message}");
            }
        });

        // طلب إذن التتبع (ATT) بعد تأخير قصير
        RequestTrackingAuthorizationAsync();

        return result;
    }

    /// <summary>
    /// طلب إذن التتبع من المستخدم (App Tracking Transparency)
    /// يُعرض بعد تأخير قصير لضمان تحميل واجهة التطبيق
    /// </summary>
    private async void RequestTrackingAuthorizationAsync()
    {
        try
        {
            // تأخير قصير لضمان ظهور واجهة التطبيق أولاً
            await Task.Delay(1500);

            var trackingService = IPlatformApplication.Current?.Services.GetService<ITrackingConsentService>();
            if (trackingService != null && !trackingService.HasRequestedConsent)
            {
                System.Diagnostics.Debug.WriteLine("[ATT] Requesting tracking authorization...");
                var status = await trackingService.RequestConsentAsync();
                System.Diagnostics.Debug.WriteLine($"[ATT] Authorization status: {status}");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ATT] Error requesting authorization: {ex.Message}");
        }
    }

    /// <summary>
    /// Handle Universal Links (iOS 9+)
    /// </summary>
    public override bool ContinueUserActivity(
        UIApplication application,
        NSUserActivity userActivity,
        UIApplicationRestorationHandler completionHandler)
    {
        if (userActivity.ActivityType == NSUserActivityType.BrowsingWeb)
        {
            var url = userActivity.WebPageUrl;
            if (url != null)
            {
                HandleDeepLink(url.ToString());
            }
        }

        return base.ContinueUserActivity(application, userActivity, completionHandler);
    }

    /// <summary>
    /// Handle Custom URL Scheme (ashare://)
    /// </summary>
    public override bool OpenUrl(
        UIApplication application,
        NSUrl url,
        NSDictionary options)
    {
        HandleDeepLink(url.ToString());
        return base.OpenUrl(application, url, options);
    }

    /// <summary>
    /// Handle deep link and capture attribution
    /// </summary>
    private void HandleDeepLink(string? url)
    {
        if (string.IsNullOrEmpty(url))
            return;

        try
        {
            System.Diagnostics.Debug.WriteLine($"[DeepLink iOS] Received: {url}");

            // التقاط بيانات الإسناد
            Task.Run(async () =>
            {
                try
                {
                    var attributionService = IPlatformApplication.Current?.Services.GetService<IAttributionCaptureService>();
                    if (attributionService != null)
                    {
                        await attributionService.CaptureFromDeepLinkAsync(url);
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[Attribution iOS] Capture error: {ex.Message}");
                }
            });

            // Navigate to the deep link path
            if (Uri.TryCreate(url, UriKind.Absolute, out var uri))
            {
                var path = uri.AbsolutePath;
                if (!string.IsNullOrEmpty(path) && path != "/")
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        // Navigate using Shell or your navigation service
                        // Shell.Current?.GoToAsync(path);
                    });
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[DeepLink iOS] Error handling: {ex.Message}");
        }
    }
}
