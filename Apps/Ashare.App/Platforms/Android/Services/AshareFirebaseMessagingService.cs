#if ANDROID
using Android.App;
using Ashare.App.Services;
using Firebase.Messaging;

namespace Ashare.App.Platforms.Android.Services;

/// <summary>
/// خدمة Firebase Messaging للحصول على التوكن ومعالجة الإشعارات
/// </summary>
[Service(Exported = true)]
[IntentFilter(new[] { "com.google.firebase.MESSAGING_EVENT" })]
public class AshareFirebaseMessagingService : FirebaseMessagingService
{
    /// <summary>
    /// يُستدعى عند تحديث التوكن
    /// </summary>
    public override void OnNewToken(string token)
    {
        base.OnNewToken(token);

        System.Diagnostics.Debug.WriteLine($"[Firebase] New token received: {token[..Math.Min(20, token.Length)]}...");

        // إرسال التوكن إلى PushNotificationService
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            try
            {
                var pushService = IPlatformApplication.Current?.Services.GetService<IPushNotificationService>();
                if (pushService != null)
                {
                    await pushService.RegisterTokenAsync(token);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Firebase] Error registering token: {ex.Message}");
            }
        });
    }

    /// <summary>
    /// يُستدعى عند استقبال رسالة
    /// </summary>
    public override void OnMessageReceived(RemoteMessage message)
    {
        base.OnMessageReceived(message);

        var notification = message.GetNotification();
        var title = notification?.Title ?? "";
        var body = notification?.Body ?? "";
        var data = message.Data?.ToDictionary(x => x.Key, x => x.Value) ?? new Dictionary<string, string>();

        System.Diagnostics.Debug.WriteLine($"[Firebase] Message received: {title}");

        // إرسال الإشعار إلى PushNotificationService
        MainThread.BeginInvokeOnMainThread(() =>
        {
            try
            {
                var pushService = IPlatformApplication.Current?.Services.GetService<IPushNotificationService>();
                if (pushService is PushNotificationService service)
                {
                    service.HandleNotificationReceived(title, body, data);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Firebase] Error handling message: {ex.Message}");
            }
        });

        // عرض الإشعار إذا كان التطبيق في الخلفية
        if (notification != null)
        {
            ShowNotification(title, body, data);
        }
    }

    private void ShowNotification(string title, string body, IDictionary<string, string> data)
    {
        var channelId = "ashare_notifications";

        var notificationBuilder = new global::Android.App.Notification.Builder(this, channelId)
            .SetContentTitle(title)
            .SetContentText(body)
            .SetSmallIcon(Resource.Mipmap.appicon)
            .SetAutoCancel(true);

        var notificationManager = (NotificationManager?)GetSystemService(NotificationService);
        notificationManager?.Notify(DateTime.Now.Millisecond, notificationBuilder.Build());
    }
}
#endif
