using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using AndroidX.Core.App;
using AndroidX.Core.Content;
using Ashare.App.Services;
using Plugin.Firebase.CloudMessaging;

namespace Ashare.App;

[Activity(
    Theme = "@style/Maui.SplashTheme",
    MainLauncher = true,
    ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density,
    LaunchMode = LaunchMode.SingleTask)]

// Deep Links - Custom Scheme
[IntentFilter(
    new[] { Intent.ActionView },
    Categories = new[] { Intent.CategoryDefault, Intent.CategoryBrowsable },
    DataScheme = "ashare")]

// App Links - HTTPS (requires assetlinks.json on server)
[IntentFilter(
    new[] { Intent.ActionView },
    Categories = new[] { Intent.CategoryDefault, Intent.CategoryBrowsable },
    DataScheme = "https",
    DataHost = "ashare.sa",
    AutoVerify = true)]
[IntentFilter(
    new[] { Intent.ActionView },
    Categories = new[] { Intent.CategoryDefault, Intent.CategoryBrowsable },
    DataScheme = "https",
    DataHost = "www.ashare.sa",
    AutoVerify = true)]

// HTTP fallback for testing
[IntentFilter(
    new[] { Intent.ActionView },
    Categories = new[] { Intent.CategoryDefault, Intent.CategoryBrowsable },
    DataScheme = "http",
    DataHost = "ashare.sa")]
public class MainActivity : MauiAppCompatActivity
{
    private const int NotificationPermissionRequestCode = 1001;

    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        // إنشاء قناة الإشعارات (مطلوب لأندرويد 8+)
        CreateNotificationChannel();

        // طلب إذن الإشعارات (أندرويد 13+)
        RequestNotificationPermission();

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
                System.Diagnostics.Debug.WriteLine($"[Attribution Android] Init error: {ex.Message}");
            }
        });

        // تهيئة خدمة الإشعارات المدفوعة
        Task.Run(async () =>
        {
            try
            {
                // تأخير قصير للسماح بتحميل الخدمات
                await Task.Delay(2000);

                var pushService = IPlatformApplication.Current?.Services.GetService<IPushNotificationService>();
                if (pushService != null)
                {
                    await pushService.InitializeAsync();
                    System.Diagnostics.Debug.WriteLine("[Push Android] Push notification service initialized");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Push Android] Init error: {ex.Message}");
            }
        });

        // Handle deep link on app launch
        HandleDeepLink(Intent);
    }

    /// <summary>
    /// إنشاء قناة الإشعارات (مطلوب لأندرويد 8.0 وأعلى)
    /// </summary>
    private void CreateNotificationChannel()
    {
        if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
        {
            var channelId = "ashare_notifications";
            var channelName = "إشعارات عشير";
            var channelDescription = "إشعارات الرسائل والحجوزات";
            var importance = NotificationImportance.High;

            var channel = new NotificationChannel(channelId, channelName, importance)
            {
                Description = channelDescription
            };

            channel.EnableVibration(true);
            channel.EnableLights(true);

            var notificationManager = (NotificationManager?)GetSystemService(NotificationService);
            notificationManager?.CreateNotificationChannel(channel);

            System.Diagnostics.Debug.WriteLine("[Push Android] Notification channel created");
        }
    }

    /// <summary>
    /// طلب إذن الإشعارات (مطلوب لأندرويد 13+)
    /// </summary>
    private void RequestNotificationPermission()
    {
        if (Build.VERSION.SdkInt >= BuildVersionCodes.Tiramisu)
        {
            if (ContextCompat.CheckSelfPermission(this, Android.Manifest.Permission.PostNotifications) != Permission.Granted)
            {
                ActivityCompat.RequestPermissions(
                    this,
                    new[] { Android.Manifest.Permission.PostNotifications },
                    NotificationPermissionRequestCode);
            }
        }
    }

    public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
    {
        base.OnRequestPermissionsResult(requestCode, permissions, grantResults);

        if (requestCode == NotificationPermissionRequestCode)
        {
            if (grantResults.Length > 0 && grantResults[0] == Permission.Granted)
            {
                System.Diagnostics.Debug.WriteLine("[Push Android] Notification permission granted");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("[Push Android] Notification permission denied");
            }
        }
    }

    protected override void OnNewIntent(Intent? intent)
    {
        base.OnNewIntent(intent);

        // Handle deep link when app is already running
        HandleDeepLink(intent);
    }

    private void HandleDeepLink(Intent? intent)
    {
        if (intent?.Action != Intent.ActionView)
            return;

        var uri = intent.Data;
        if (uri == null)
            return;

        try
        {
            var url = uri.ToString();
            System.Diagnostics.Debug.WriteLine($"[DeepLink Android] Received: {url}");

            // التقاط بيانات الإسناد
            Task.Run(async () =>
            {
                try
                {
                    var attributionService = IPlatformApplication.Current?.Services.GetService<IAttributionCaptureService>();
                    if (attributionService != null && !string.IsNullOrEmpty(url))
                    {
                        await attributionService.CaptureFromDeepLinkAsync(url);
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[Attribution Android] Capture error: {ex.Message}");
                }
            });

            // Navigate to the deep link path
            var path = uri.Path;
            if (!string.IsNullOrEmpty(path))
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    // Navigate using Shell or your navigation service
                    // Shell.Current?.GoToAsync(path);
                });
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[DeepLink Android] Error handling: {ex.Message}");
        }
    }
}
