using Ashare.App.Services;
using Ashare.Shared.Services;
using Foundation;
using UIKit;
using UserNotifications;
using Firebase.Core;
using Firebase.CloudMessaging;

namespace Ashare.App;

[Register("AppDelegate")]
public class AppDelegate : MauiUIApplicationDelegate, IUNUserNotificationCenterDelegate
{
    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

    public override bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
    {
        // ✅ تهيئة Firebase (مطلوب قبل أي عملية Firebase)
        try
        {
            App.Configure();
            System.Diagnostics.Debug.WriteLine("[Firebase iOS] Firebase configured successfully");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Firebase iOS] Firebase configuration error: {ex.Message}");
        }

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

        // تسجيل للإشعارات المدفوعة
        RegisterForPushNotifications(application);

        return result;
    }

    /// <summary>
    /// تسجيل التطبيق للإشعارات المدفوعة
    /// </summary>
    private void RegisterForPushNotifications(UIApplication application)
    {
        UNUserNotificationCenter.Current.Delegate = this;

        UNUserNotificationCenter.Current.RequestAuthorization(
            UNAuthorizationOptions.Alert | UNAuthorizationOptions.Badge | UNAuthorizationOptions.Sound,
            (granted, error) =>
            {
                if (granted)
                {
                    System.Diagnostics.Debug.WriteLine("[Push iOS] Notification authorization granted");
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        application.RegisterForRemoteNotifications();
                    });

                    // تهيئة خدمة الإشعارات بعد الحصول على الإذن
                    Task.Run(async () =>
                    {
                        try
                        {
                            await Task.Delay(2000);
                            var pushService = IPlatformApplication.Current?.Services.GetService<IPushNotificationService>();
                            if (pushService != null)
                            {
                                await pushService.InitializeAsync();
                                System.Diagnostics.Debug.WriteLine("[Push iOS] Push notification service initialized");
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"[Push iOS] Init error: {ex.Message}");
                        }
                    });
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"[Push iOS] Notification authorization denied: {error?.LocalizedDescription}");
                }
            });
    }

    /// <summary>
    /// استلام Device Token من APNS
    /// ⚠️ مطلوب لأن FirebaseAppDelegateProxyEnabled=false في Info.plist
    /// </summary>
    [Export("application:didRegisterForRemoteNotificationsWithDeviceToken:")]
    public void RegisteredForRemoteNotifications(UIApplication application, NSData deviceToken)
    {
        System.Diagnostics.Debug.WriteLine("[Push iOS] Registered for remote notifications with APNs token");

        // ⚠️ مهم: تمرير APNs token إلى Firebase يدوياً
        // لأن FirebaseAppDelegateProxyEnabled=false يعطل التمرير التلقائي
        Messaging.SharedInstance.ApnsToken = deviceToken;

        System.Diagnostics.Debug.WriteLine("[Push iOS] APNs token passed to Firebase Messaging");
    }

    /// <summary>
    /// فشل التسجيل للإشعارات
    /// </summary>
    [Export("application:didFailToRegisterForRemoteNotificationsWithError:")]
    public void FailedToRegisterForRemoteNotifications(UIApplication application, NSError error)
    {
        System.Diagnostics.Debug.WriteLine($"[Push iOS] Failed to register: {error.LocalizedDescription}");
    }

    /// <summary>
    /// استلام إشعار عندما التطبيق في الواجهة
    /// </summary>
    [Export("userNotificationCenter:willPresentNotification:withCompletionHandler:")]
    public void WillPresentNotification(UNUserNotificationCenter center, UNNotification notification, Action<UNNotificationPresentationOptions> completionHandler)
    {
        System.Diagnostics.Debug.WriteLine($"[Push iOS] Notification received in foreground: {notification.Request.Content.Title}");

        // عرض الإشعار حتى عندما التطبيق مفتوح
        completionHandler(UNNotificationPresentationOptions.Banner | UNNotificationPresentationOptions.Sound | UNNotificationPresentationOptions.Badge);
    }

    /// <summary>
    /// معالجة الضغط على الإشعار
    /// </summary>
    [Export("userNotificationCenter:didReceiveNotificationResponse:withCompletionHandler:")]
    public void DidReceiveNotificationResponse(UNUserNotificationCenter center, UNNotificationResponse response, Action completionHandler)
    {
        var userInfo = response.Notification.Request.Content.UserInfo;
        System.Diagnostics.Debug.WriteLine($"[Push iOS] Notification tapped: {response.Notification.Request.Content.Title}");

        // يمكن إضافة التنقل بناءً على بيانات الإشعار هنا

        completionHandler();
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
