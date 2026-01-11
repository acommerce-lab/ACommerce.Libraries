using ACommerce.Client.Notifications;
using Microsoft.Extensions.Logging;
using Plugin.Firebase.CloudMessaging;
using Plugin.Firebase.CloudMessaging.EventArgs;

namespace Ashare.App.Services;

/// <summary>
/// خدمة إشعارات Firebase Push للتطبيق
/// تتولى تسجيل الجهاز واستلام الإشعارات
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
    /// تهيئة خدمة الإشعارات وتسجيل الجهاز
    /// </summary>
    public async Task InitializeAsync()
    {
        if (_isInitialized)
            return;

        try
        {
            _logger.LogInformation("[Push] Initializing Firebase Cloud Messaging...");

            // طلب إذن الإشعارات
            await CrossFirebaseCloudMessaging.Current.CheckIfValidAsync();

            // الاشتراك في التحديثات عند تجديد التوكن
            CrossFirebaseCloudMessaging.Current.TokenChanged += OnTokenChanged;

            // الاشتراك في استقبال الإشعارات
            CrossFirebaseCloudMessaging.Current.NotificationReceived += OnNotificationReceived;
            CrossFirebaseCloudMessaging.Current.NotificationTapped += OnNotificationTapped;

            // الحصول على التوكن الحالي
            _currentToken = await CrossFirebaseCloudMessaging.Current.GetTokenAsync();

            if (!string.IsNullOrEmpty(_currentToken))
            {
                _logger.LogInformation("[Push] FCM Token obtained: {Token}", _currentToken[..Math.Min(20, _currentToken.Length)] + "...");
                await RegisterTokenWithBackendAsync(_currentToken);
            }
            else
            {
                _logger.LogWarning("[Push] Failed to get FCM token");
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
    /// الحصول على التوكن الحالي
    /// </summary>
    public string? GetCurrentToken() => _currentToken;

    /// <summary>
    /// إعادة تسجيل التوكن مع الخادم (بعد تسجيل الدخول مثلاً)
    /// </summary>
    public async Task RefreshTokenRegistrationAsync()
    {
        if (!string.IsNullOrEmpty(_currentToken))
        {
            await RegisterTokenWithBackendAsync(_currentToken);
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

            _logger.LogInformation("[Push] Device token registered with backend for platform: {Platform}", platform);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Push] Failed to register device token with backend");
        }
    }

    private async void OnTokenChanged(object? sender, string newToken)
    {
        _logger.LogInformation("[Push] FCM Token refreshed");
        _currentToken = newToken;

        await RegisterTokenWithBackendAsync(newToken);
        TokenRefreshed?.Invoke(this, newToken);
    }

    private void OnNotificationReceived(object? sender, FCMNotificationReceivedEventArgs e)
    {
        _logger.LogInformation("[Push] Notification received: {Title}", e.Notification?.Title);

        NotificationReceived?.Invoke(this, new PushNotificationEventArgs
        {
            Title = e.Notification?.Title ?? "",
            Body = e.Notification?.Body ?? "",
            Data = e.Notification?.Data ?? new Dictionary<string, string>(),
            WasInForeground = true
        });
    }

    private void OnNotificationTapped(object? sender, FCMNotificationTappedEventArgs e)
    {
        _logger.LogInformation("[Push] Notification tapped: {Title}", e.Notification?.Title);

        var data = e.Notification?.Data ?? new Dictionary<string, string>();

        // التنقل بناءً على نوع الإشعار
        MainThread.BeginInvokeOnMainThread(() =>
        {
            HandleNotificationNavigation(data);
        });
    }

    private void HandleNotificationNavigation(IDictionary<string, string> data)
    {
        try
        {
            if (data.TryGetValue("type", out var type))
            {
                string? targetPage = type switch
                {
                    "NewBooking" => "/bookings",
                    "BookingConfirmed" => "/bookings",
                    "BookingRejected" => "/bookings",
                    "ChatMessage" => data.TryGetValue("chatId", out var chatId) ? $"/chat/{chatId}" : "/chats",
                    "NewMessage" => data.TryGetValue("chatId", out var cid) ? $"/chat/{cid}" : "/chats",
                    _ => null
                };

                if (!string.IsNullOrEmpty(targetPage))
                {
                    _logger.LogInformation("[Push] Navigating to: {Page}", targetPage);
                    // يمكن استخدام IAppNavigationService هنا للتنقل
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Push] Error handling notification navigation");
        }
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
