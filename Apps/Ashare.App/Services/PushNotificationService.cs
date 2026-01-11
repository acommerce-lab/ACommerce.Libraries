using ACommerce.Client.Notifications;
using Microsoft.Extensions.Logging;

namespace Ashare.App.Services;

/// <summary>
/// خدمة إشعارات Push للتطبيق
/// ملاحظة: هذه نسخة مبسطة. Firebase SDK سيُضاف لاحقاً عند البناء على macOS
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
            _logger.LogInformation("[Push] Initializing Push Notification Service...");

            // TODO: تكامل Firebase سيُضاف لاحقاً
            // حالياً الخدمة جاهزة لاستقبال التوكن من native code

            _isInitialized = true;
            _logger.LogInformation("[Push] Push Notification Service initialized (stub mode)");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Push] Failed to initialize Push Notification Service");
        }

        await Task.CompletedTask;
    }

    /// <summary>
    /// الحصول على التوكن الحالي
    /// </summary>
    public string? GetCurrentToken() => _currentToken;

    /// <summary>
    /// تسجيل التوكن من native code
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
    }

    /// <summary>
    /// الاشتراك في موضوع معين
    /// </summary>
    public Task SubscribeToTopicAsync(string topic)
    {
        _logger.LogInformation("[Push] Subscribe to topic requested: {Topic} (not implemented)", topic);
        return Task.CompletedTask;
    }

    /// <summary>
    /// إلغاء الاشتراك من موضوع
    /// </summary>
    public Task UnsubscribeFromTopicAsync(string topic)
    {
        _logger.LogInformation("[Push] Unsubscribe from topic requested: {Topic} (not implemented)", topic);
        return Task.CompletedTask;
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
