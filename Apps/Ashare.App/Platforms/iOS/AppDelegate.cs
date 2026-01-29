using Ashare.App.Services;
using Ashare.Shared.Services;
using Foundation;
using UIKit;
using UserNotifications;
using Firebase.Core;
using System.Text;
using System.Text.Json;
using Plugin.Firebase.Core.Platforms.iOS;

namespace Ashare.App;

[Register("AppDelegate")]
public class AppDelegate : MauiUIApplicationDelegate, IUNUserNotificationCenterDelegate
{
    private static readonly HttpClient _httpClient = new();
    private const string DiagnosticUrl = "https://api.ashare.sa/api/errorreporting/report";

    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

    public override bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
    {
        // ✅ تهيئة Firebase Native SDK أولاً
        try
        {
            Firebase.Core.App.Configure();
            System.Diagnostics.Debug.WriteLine("[Firebase iOS] Firebase.Core configured successfully");
            _ = SendDiagnosticAsync("Firebase.Configure", "SUCCESS", "Firebase.Core configured successfully");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Firebase iOS] Firebase.Core configuration error: {ex.Message}");
            _ = SendDiagnosticAsync("Firebase.Configure", "FAILED", ex.Message, ex.StackTrace);
        }

        var result = base.FinishedLaunching(application, launchOptions);

        // ✅ تهيئة CrossFirebase بعد base.FinishedLaunching لضمان اكتمال تهيئة MAUI
        try
        {
            CrossFirebase.Initialize();
            System.Diagnostics.Debug.WriteLine("[Firebase iOS] CrossFirebase initialized successfully");
            _ = SendDiagnosticAsync("CrossFirebase.Initialize", "SUCCESS", "CrossFirebase initialized successfully");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Firebase iOS] CrossFirebase initialization error: {ex.Message}");
            _ = SendDiagnosticAsync("CrossFirebase.Initialize", "FAILED", ex.Message, ex.StackTrace);
        }

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

        _ = SendDiagnosticAsync("Push.RequestAuthorization", "STARTED", "Requesting notification authorization...");

        UNUserNotificationCenter.Current.RequestAuthorization(
            UNAuthorizationOptions.Alert | UNAuthorizationOptions.Badge | UNAuthorizationOptions.Sound,
            (granted, error) =>
            {
                if (granted)
                {
                    System.Diagnostics.Debug.WriteLine("[Push iOS] Notification authorization granted");
                    _ = SendDiagnosticAsync("Push.Authorization", "GRANTED", "User granted notification permission");

                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        application.RegisterForRemoteNotifications();
                        _ = SendDiagnosticAsync("Push.RegisterForRemote", "CALLED", "RegisterForRemoteNotifications called");
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
                                await SendDiagnosticAsync("Push.InitializeService", "STARTED", "Initializing PushNotificationService...");
                                await pushService.InitializeAsync();
                                await SendDiagnosticAsync("Push.InitializeService", "COMPLETED", "PushNotificationService initialized");
                                System.Diagnostics.Debug.WriteLine("[Push iOS] Push notification service initialized");
                            }
                            else
                            {
                                await SendDiagnosticAsync("Push.InitializeService", "FAILED", "PushNotificationService is NULL!");
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"[Push iOS] Init error: {ex.Message}");
                            await SendDiagnosticAsync("Push.InitializeService", "ERROR", ex.Message, ex.StackTrace);
                        }
                    });
                }
                else
                {
                    var errorMsg = error?.LocalizedDescription ?? "Unknown error";
                    System.Diagnostics.Debug.WriteLine($"[Push iOS] Notification authorization denied: {errorMsg}");
                    _ = SendDiagnosticAsync("Push.Authorization", "DENIED", errorMsg);
                }
            });
    }

    /// <summary>
    /// استلام Device Token من APNS
    /// </summary>
    [Export("application:didRegisterForRemoteNotificationsWithDeviceToken:")]
    public void RegisteredForRemoteNotifications(UIApplication application, NSData deviceToken)
    {
        var tokenBytes = deviceToken.ToArray();
        var tokenString = BitConverter.ToString(tokenBytes).Replace("-", "").ToLowerInvariant();
        System.Diagnostics.Debug.WriteLine($"[Push iOS] APNs token received: {tokenString[..Math.Min(20, tokenString.Length)]}...");
        _ = SendDiagnosticAsync("Push.APNsToken", "RECEIVED", $"APNs token: {tokenString[..Math.Min(40, tokenString.Length)]}...");
    }

    /// <summary>
    /// فشل التسجيل للإشعارات
    /// </summary>
    [Export("application:didFailToRegisterForRemoteNotificationsWithError:")]
    public void FailedToRegisterForRemoteNotifications(UIApplication application, NSError error)
    {
        System.Diagnostics.Debug.WriteLine($"[Push iOS] Failed to register: {error.LocalizedDescription}");
        _ = SendDiagnosticAsync("Push.APNsToken", "FAILED", error.LocalizedDescription, $"Code: {error.Code}, Domain: {error.Domain}");
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

    /// <summary>
    /// إرسال تقرير تشخيصي للخادم
    /// </summary>
    private static async Task SendDiagnosticAsync(string operation, string status, string message, string? stackTrace = null)
    {
        try
        {
            var report = new
            {
                ReportId = Guid.NewGuid().ToString(),
                Source = "iOS-Push-Diagnostic",
                Operation = $"{operation}: {status}",
                ErrorMessage = message,
                StackTrace = stackTrace,
                Platform = "iOS",
                AppVersion = AppInfo.VersionString,
                OsVersion = $"iOS {UIDevice.CurrentDevice.SystemVersion}",
                DeviceModel = UIDevice.CurrentDevice.Model,
                Timestamp = DateTime.UtcNow,
                AdditionalData = new Dictionary<string, object>
                {
                    ["DeviceName"] = UIDevice.CurrentDevice.Name,
                    ["SystemName"] = UIDevice.CurrentDevice.SystemName,
                    ["Idiom"] = UIDevice.CurrentDevice.UserInterfaceIdiom.ToString()
                }
            };

            var json = JsonSerializer.Serialize(report);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(DiagnosticUrl, content);
            System.Diagnostics.Debug.WriteLine($"[Diagnostic] Sent: {operation} = {status}, Response: {response.StatusCode}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Diagnostic] Failed to send: {ex.Message}");
        }
    }
}
