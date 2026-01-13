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
    private bool _isInitialized;

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
    /// تهيئة خدمة الإشعارات وتسجيل الجهاز مع Firebase
    /// </summary>
    public async Task InitializeAsync()
    {
        if (_isInitialized)
            return;

        try
        {
            _logger.LogInformation("[Push] Initializing Firebase Cloud Messaging...");

            // التحقق من دعم الإشعارات
            await CrossFirebaseCloudMessaging.Current.CheckIfValidAsync();

            // الاشتراك في تحديثات التوكن
            CrossFirebaseCloudMessaging.Current.TokenChanged += OnTokenChanged;

            // الاشتراك في استقبال الإشعارات
            CrossFirebaseCloudMessaging.Current.NotificationReceived += OnNotificationReceived;
            CrossFirebaseCloudMessaging.Current.NotificationTapped += OnNotificationTapped;

            // الحصول على التوكن الحالي
            var token = await CrossFirebaseCloudMessaging.Current.GetTokenAsync();
            if (!string.IsNullOrEmpty(token))
            {
                _currentToken = token;
                _logger.LogInformation("[Push] Firebase token obtained: {TokenPrefix}...", token[..Math.Min(20, token.Length)]);

                // تسجيل التوكن مع الخادم
                await RegisterTokenWithBackendAsync(token);
            }
            else
            {
                _logger.LogWarning("[Push] Firebase token is null or empty");
            }

            _isInitialized = true;
            _logger.LogInformation("[Push] Firebase Cloud Messaging initialized successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Push] Failed to initialize Firebase Cloud Messaging");
        }
    }

    /// <summary>
    /// معالجة تغيير التوكن
    /// </summary>
    private async void OnTokenChanged(object? sender, Plugin.Firebase.CloudMessaging.EventArgs.FCMTokenChangedEventArgs e)
    {
        try
        {
            var newToken = e.Token;
            if (string.IsNullOrEmpty(newToken))
                return;

            _logger.LogInformation("[Push] Firebase token refreshed");
            _currentToken = newToken;

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
    /// </summary>
    public async Task RefreshTokenRegistrationAsync()
    {
        if (!string.IsNullOrEmpty(_currentToken))
        {
            await RegisterTokenWithBackendAsync(_currentToken);
        }
        else
        {
            // حاول الحصول على التوكن مرة أخرى
            try
            {
                var token = await CrossFirebaseCloudMessaging.Current.GetTokenAsync();
                if (!string.IsNullOrEmpty(token))
                {
                    _currentToken = token;
                    await RegisterTokenWithBackendAsync(token);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Push] Failed to refresh token");
            }
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
                Platform = platform,
                AppVersion = AppInfo.VersionString,
                DeviceModel = DeviceInfo.Model
            });

            _logger.LogInformation("[Push] Device token registered with backend for platform: {Platform}", platform);
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
    /// <summary>
    /// تهيئة الخدمة وتسجيل الجهاز
    /// </summary>
    Task InitializeAsync();

    /// <summary>
    /// الحصول على التوكن الحالي
    /// </summary>
    string? GetCurrentToken();

    /// <summary>
    /// تسجيل التوكن من native code
    /// </summary>
    Task RegisterTokenAsync(string token);

    /// <summary>
    /// إعادة تسجيل التوكن
    /// </summary>
    Task RefreshTokenRegistrationAsync();

    /// <summary>
    /// الاشتراك في موضوع
    /// </summary>
    Task SubscribeToTopicAsync(string topic);

    /// <summary>
    /// إلغاء الاشتراك من موضوع
    /// </summary>
    Task UnsubscribeFromTopicAsync(string topic);

    /// <summary>
    /// حدث استقبال إشعار
    /// </summary>
    event EventHandler<PushNotificationEventArgs>? NotificationReceived;

    /// <summary>
    /// حدث تجديد التوكن
    /// </summary>
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
