using ACommerce.Client.Notifications;
using Microsoft.Extensions.Logging;
using Plugin.Firebase.CloudMessaging;

namespace Ashare.App.Services;

/// <summary>
/// خدمة إشعارات Push للتطبيق مع تكامل Firebase Cloud Messaging
/// </summary>
public class PushNotificationService : IPushNotificationService
{
    private readonly NotificationsClient _notificationsClient;
    private readonly ILogger<PushNotificationService> _logger;
    private string? _currentToken;
    private bool _isSubscribed;

    // مفاتيح التخزين المحلي
    private const string TokenKey = "firebase_token";
    private const string TokenExpiryKey = "firebase_token_expiry";

    // مدة صلاحية التوكن (30 يوم)
    private static readonly TimeSpan TokenValidity = TimeSpan.FromDays(30);

    public event EventHandler<PushNotificationEventArgs>? NotificationReceived;
    public event EventHandler<string>? TokenRefreshed;

    public PushNotificationService(
        NotificationsClient notificationsClient,
        ILogger<PushNotificationService> logger)
    {
        _notificationsClient = notificationsClient;
        _logger = logger;
    }

    /// <summary>
    /// تهيئة خدمة الإشعارات - يتحقق من التوكن ويطلب جديد إذا لزم
    /// </summary>
    public async Task InitializeAsync()
    {
        try
        {
            _logger.LogInformation("[Push] Initializing Firebase Cloud Messaging...");

            // التحقق من دعم الإشعارات
            await CrossFirebaseCloudMessaging.Current.CheckIfValidAsync();

            // الاشتراك في الأحداث (مرة واحدة فقط)
            if (!_isSubscribed)
            {
                CrossFirebaseCloudMessaging.Current.TokenChanged += OnTokenChanged;
                CrossFirebaseCloudMessaging.Current.NotificationReceived += OnNotificationReceived;
                CrossFirebaseCloudMessaging.Current.NotificationTapped += OnNotificationTapped;
                _isSubscribed = true;
            }

            // التحقق من التوكن المحفوظ
            string? storedToken = null;
            DateTime tokenExpiry = DateTime.MinValue;

            try
            {
                storedToken = GetStoredToken();
                tokenExpiry = GetStoredTokenExpiry();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[Push] Failed to read stored token, will request new one");
            }

            if (!string.IsNullOrEmpty(storedToken) && tokenExpiry > DateTime.UtcNow)
            {
                // ✅ التوكن موجود وصالح - نستخدمه
                _currentToken = storedToken;
                _logger.LogInformation("[Push] Using stored token (expires: {Expiry})", tokenExpiry);
            }
            else
            {
                // 🔄 التوكن غير موجود أو منتهي - نطلب جديد
                _logger.LogInformation("[Push] Token missing or expired, requesting new token...");
                await RequestAndRegisterNewTokenAsync();
            }

            _logger.LogInformation("[Push] Firebase Cloud Messaging initialized successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Push] Failed to initialize Firebase Cloud Messaging");
        }
    }

    /// <summary>
    /// طلب توكن جديد من Firebase وتسجيله مع الخادم
    /// </summary>
    private async Task RequestAndRegisterNewTokenAsync()
    {
        try
        {
            var token = await CrossFirebaseCloudMessaging.Current.GetTokenAsync();

            if (!string.IsNullOrEmpty(token))
            {
                _currentToken = token;
                var expiry = DateTime.UtcNow.Add(TokenValidity);

                // حفظ التوكن محلياً مع تاريخ الانتهاء
                SaveTokenLocally(token, expiry);

                _logger.LogInformation("[Push] New token obtained: {TokenPrefix}..., expires: {Expiry}",
                    token.Length > 20 ? token[..20] : token,
                    expiry);

                // تسجيل مع الخادم
                await RegisterTokenWithBackendAsync(token);

                TokenRefreshed?.Invoke(this, token);
            }
            else
            {
                _logger.LogWarning("[Push] Firebase returned null or empty token");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Push] Failed to request new token");
        }
    }

    /// <summary>
    /// حفظ التوكن محلياً (متزامن - Preferences فقط لتجنب مشاكل SecureStorage)
    /// </summary>
    private void SaveTokenLocally(string token, DateTime expiry)
    {
        try
        {
            Preferences.Default.Set(TokenKey, token);
            Preferences.Default.Set(TokenExpiryKey, expiry.ToString("O"));
            _logger.LogDebug("[Push] Token saved locally");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "[Push] Failed to save token locally");
        }
    }

    /// <summary>
    /// الحصول على التوكن المحفوظ (متزامن)
    /// </summary>
    private string? GetStoredToken()
    {
        try
        {
            return Preferences.Default.Get(TokenKey, string.Empty);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// الحصول على تاريخ انتهاء التوكن المحفوظ (متزامن)
    /// </summary>
    private DateTime GetStoredTokenExpiry()
    {
        try
        {
            var expiryStr = Preferences.Default.Get(TokenExpiryKey, string.Empty);

            if (!string.IsNullOrEmpty(expiryStr) && DateTime.TryParse(expiryStr, out var expiry))
            {
                return expiry;
            }
        }
        catch { }

        return DateTime.MinValue;
    }

    /// <summary>
    /// معالجة تغيير التوكن (يُستدعى تلقائياً من Firebase عند تغيير التوكن)
    /// </summary>
    private async void OnTokenChanged(object? sender, Plugin.Firebase.CloudMessaging.EventArgs.FCMTokenChangedEventArgs e)
    {
        try
        {
            var newToken = e.Token;
            if (string.IsNullOrEmpty(newToken))
                return;

            _logger.LogInformation("[Push] Firebase token changed by Firebase");
            _currentToken = newToken;

            var expiry = DateTime.UtcNow.Add(TokenValidity);

            // حفظ التوكن الجديد محلياً
            SaveTokenLocally(newToken, expiry);

            // تسجيل مع الخادم
            await RegisterTokenWithBackendAsync(newToken);

            TokenRefreshed?.Invoke(this, newToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Push] Error handling token change");
        }
    }

    /// <summary>
    /// معالجة استقبال إشعار (التطبيق في المقدمة)
    /// </summary>
    private void OnNotificationReceived(object? sender, Plugin.Firebase.CloudMessaging.EventArgs.FCMNotificationReceivedEventArgs e)
    {
        try
        {
            var notification = e.Notification;
            _logger.LogInformation("[Push] Notification received in foreground: {Title}", notification.Title);

            NotificationReceived?.Invoke(this, new PushNotificationEventArgs
            {
                Title = notification.Title ?? "",
                Body = notification.Body ?? "",
                Data = notification.Data ?? new Dictionary<string, string>(),
                WasInForeground = true
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Push] Error handling received notification");
        }
    }

    /// <summary>
    /// معالجة النقر على إشعار
    /// </summary>
    private void OnNotificationTapped(object? sender, Plugin.Firebase.CloudMessaging.EventArgs.FCMNotificationTappedEventArgs e)
    {
        try
        {
            var notification = e.Notification;
            _logger.LogInformation("[Push] Notification tapped: {Title}", notification.Title);

            NotificationReceived?.Invoke(this, new PushNotificationEventArgs
            {
                Title = notification.Title ?? "",
                Body = notification.Body ?? "",
                Data = notification.Data ?? new Dictionary<string, string>(),
                WasInForeground = false
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Push] Error handling notification tap");
        }
    }

    /// <summary>
    /// الحصول على التوكن الحالي
    /// </summary>
    public string? GetCurrentToken() => _currentToken;

    /// <summary>
    /// تسجيل التوكن من native code (للتوافق مع الكود القديم)
    /// </summary>
    public async Task RegisterTokenAsync(string token)
    {
        if (string.IsNullOrEmpty(token))
            return;

        _currentToken = token;
        await RegisterTokenWithBackendAsync(token);
        TokenRefreshed?.Invoke(this, token);
    }

    /// <summary>
    /// إعادة تسجيل التوكن مع الخادم (بعد تسجيل الدخول مثلاً)
    /// يطلب توكن جديد إذا كان الحالي منتهي
    /// </summary>
    public async Task RefreshTokenRegistrationAsync()
    {
        var tokenExpiry = GetStoredTokenExpiry();

        if (!string.IsNullOrEmpty(_currentToken) && tokenExpiry > DateTime.UtcNow)
        {
            // التوكن صالح - نرسله للخادم
            await RegisterTokenWithBackendAsync(_currentToken);
        }
        else
        {
            // التوكن منتهي أو غير موجود - نطلب جديد
            await RequestAndRegisterNewTokenAsync();
        }
    }

    /// <summary>
    /// الاشتراك في موضوع معين
    /// </summary>
    public async Task SubscribeToTopicAsync(string topic)
    {
        try
        {
            await CrossFirebaseCloudMessaging.Current.SubscribeToTopicAsync(topic);
            _logger.LogInformation("[Push] Subscribed to topic: {Topic}", topic);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Push] Failed to subscribe to topic: {Topic}", topic);
        }
    }

    /// <summary>
    /// إلغاء الاشتراك من موضوع
    /// </summary>
    public async Task UnsubscribeFromTopicAsync(string topic)
    {
        try
        {
            await CrossFirebaseCloudMessaging.Current.UnsubscribeFromTopicAsync(topic);
            _logger.LogInformation("[Push] Unsubscribed from topic: {Topic}", topic);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Push] Failed to unsubscribe from topic: {Topic}", topic);
        }
    }

    private async Task RegisterTokenWithBackendAsync(string token)
    {
        try
        {
            var platform = DeviceInfo.Platform == DevicePlatform.iOS ? "iOS" : "Android";

            await _notificationsClient.RegisterDeviceTokenAsync(new RegisterDeviceTokenRequest
            {
                DeviceToken = token,
                Platform = platform
            });

            _logger.LogInformation("[Push] ✅ Device token registered with backend for platform: {Platform}", platform);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Push] Failed to register device token with backend");
        }
    }

    /// <summary>
    /// معالجة إشعار وارد (يُستدعى من native code)
    /// </summary>
    public void HandleNotificationReceived(string title, string body, IDictionary<string, string>? data)
    {
        _logger.LogInformation("[Push] Notification received: {Title}", title);

        NotificationReceived?.Invoke(this, new PushNotificationEventArgs
        {
            Title = title,
            Body = body,
            Data = data ?? new Dictionary<string, string>(),
            WasInForeground = true
        });
    }
}

/// <summary>
/// واجهة خدمة الإشعارات المدفوعة
/// </summary>
public interface IPushNotificationService
{
    Task InitializeAsync();
    string? GetCurrentToken();
    Task RegisterTokenAsync(string token);
    Task RefreshTokenRegistrationAsync();
    Task SubscribeToTopicAsync(string topic);
    Task UnsubscribeFromTopicAsync(string topic);
    event EventHandler<PushNotificationEventArgs>? NotificationReceived;
    event EventHandler<string>? TokenRefreshed;
}

/// <summary>
/// بيانات حدث الإشعار
/// </summary>
public class PushNotificationEventArgs : EventArgs
{
    public string Title { get; set; } = "";
    public string Body { get; set; } = "";
    public IDictionary<string, string> Data { get; set; } = new Dictionary<string, string>();
    public bool WasInForeground { get; set; }
}
