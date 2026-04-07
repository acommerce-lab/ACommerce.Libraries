using ACommerce.Notifications.Abstractions.Contracts;
using ACommerce.Notifications.Abstractions.Enums;
using ACommerce.Notifications.Abstractions.Models;
using ACommerce.Notifications.Channels.Sms.Gateways;
using ACommerce.Notifications.Channels.Sms.Options;
using Microsoft.Extensions.Logging;

namespace ACommerce.Notifications.Channels.Sms;

/// <summary>
/// قناة إشعارات SMS.
/// تتوقع أن يحتوي Notification.Data على مفتاح "Phone" برقم المستلم.
/// </summary>
public class SmsNotificationChannel : INotificationChannel
{
    private readonly ISmsGateway _gateway;
    private readonly SmsOptions _options;
    private readonly ILogger<SmsNotificationChannel> _logger;

    public NotificationChannel Channel => NotificationChannel.SMS;

    public SmsNotificationChannel(
        ISmsGateway gateway,
        SmsOptions options,
        ILogger<SmsNotificationChannel> logger)
    {
        _gateway = gateway ?? throw new ArgumentNullException(nameof(gateway));
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<NotificationResult> SendAsync(
        Notification notification,
        CancellationToken cancellationToken = default)
    {
        var phone = GetPhoneNumber(notification);

        if (string.IsNullOrWhiteSpace(phone))
        {
            _logger.LogWarning("[SMS] No phone number for notification {Id} (user {UserId})",
                notification.Id, notification.UserId);

            return new NotificationResult
            {
                Success = false,
                NotificationId = notification.Id,
                ErrorMessage = "Phone number not provided. Add 'Phone' key to Notification.Data."
            };
        }

        var message = BuildSmsText(notification);

        _logger.LogDebug("[SMS] Sending to {Phone} | {Title}", phone, notification.Title);

        var result = await _gateway.SendAsync(
            phone,
            message,
            _options.SenderName ?? _options.SenderNumber,
            cancellationToken);

        if (result.Success)
        {
            _logger.LogInformation("[SMS] Delivered to {Phone} for notification {Id}", phone, notification.Id);
            return new NotificationResult
            {
                Success = true,
                NotificationId = notification.Id,
                SentAt = DateTimeOffset.UtcNow,
                Metadata = result.MessageId != null
                    ? new Dictionary<string, object> { ["messageId"] = result.MessageId }
                    : null
            };
        }

        _logger.LogWarning("[SMS] Failed to deliver to {Phone}: {Error}", phone, result.Error);
        return new NotificationResult
        {
            Success = false,
            NotificationId = notification.Id,
            ErrorMessage = result.Error
        };
    }

    public Task<bool> ValidateAsync(Notification notification, CancellationToken cancellationToken = default)
    {
        var phone = GetPhoneNumber(notification);
        return Task.FromResult(!string.IsNullOrWhiteSpace(phone));
    }

    // =============================================
    // Private Helpers
    // =============================================

    private static string? GetPhoneNumber(Notification notification)
    {
        if (notification.Data == null) return null;

        // Try common keys: Phone, phone, PhoneNumber, Mobile
        foreach (var key in new[] { "Phone", "phone", "PhoneNumber", "Mobile", "mobile" })
        {
            if (notification.Data.TryGetValue(key, out var phone) && !string.IsNullOrWhiteSpace(phone))
                return phone;
        }

        return null;
    }

    private static string BuildSmsText(Notification notification)
    {
        // إذا كانت الرسالة قصيرة اكتفِ بها، وإلا أضف العنوان
        if (string.IsNullOrWhiteSpace(notification.Title))
            return notification.Message;

        // SMS لها حد 160 حرف لكل segment - لا نقطع، ندع المزود يعالج ذلك
        return $"{notification.Title}\n{notification.Message}";
    }
}
