using ACommerce.Notifications.Abstractions.Contracts;
using ACommerce.Notifications.Abstractions.Enums;
using ACommerce.Notifications.Abstractions.Models;
using ACommerce.Notifications.Channels.WhatsApp.Gateways;
using ACommerce.Notifications.Channels.WhatsApp.Options;
using Microsoft.Extensions.Logging;

namespace ACommerce.Notifications.Channels.WhatsApp;

/// <summary>
/// قناة إشعارات WhatsApp Business.
///
/// تتوقع Notification.Data:
///   "Phone" أو "WhatsApp" → رقم المستلم (بصيغة دولية)
///   "Template" → اسم القالب (اختياري - إذا غاب يُرسل نص مباشر)
///   "LanguageCode" → رمز اللغة (افتراضي: "ar")
///   "Params" → معاملات القالب مفصولة بـ | (اختياري)
/// </summary>
public class WhatsAppNotificationChannel : INotificationChannel
{
    private readonly IWhatsAppGateway _gateway;
    private readonly WhatsAppOptions _options;
    private readonly ILogger<WhatsAppNotificationChannel> _logger;

    public NotificationChannel Channel => NotificationChannel.WhatsApp;

    public WhatsAppNotificationChannel(
        IWhatsAppGateway gateway,
        WhatsAppOptions options,
        ILogger<WhatsAppNotificationChannel> logger)
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
            _logger.LogWarning("[WhatsApp] No phone for notification {Id} (user {UserId})",
                notification.Id, notification.UserId);
            return new NotificationResult
            {
                Success = false,
                NotificationId = notification.Id,
                ErrorMessage = "WhatsApp phone number not provided. Add 'Phone' or 'WhatsApp' key to Notification.Data."
            };
        }

        WhatsAppResult result;
        var templateName = notification.Data?.GetValueOrDefault("Template");

        if (!string.IsNullOrWhiteSpace(templateName))
        {
            // إرسال بقالب
            var langCode = notification.Data?.GetValueOrDefault("LanguageCode") ?? "ar";
            var paramsRaw = notification.Data?.GetValueOrDefault("Params");
            var parameters = paramsRaw?.Split('|', StringSplitOptions.RemoveEmptyEntries);

            _logger.LogDebug("[WhatsApp] Sending template '{Template}' to {Phone}", templateName, phone);
            result = await _gateway.SendTemplateAsync(phone, templateName, langCode, parameters, cancellationToken);
        }
        else
        {
            // إرسال نص مباشر
            var text = BuildWhatsAppText(notification);
            _logger.LogDebug("[WhatsApp] Sending text to {Phone}", phone);
            result = await _gateway.SendTextAsync(phone, text, cancellationToken);
        }

        if (result.Success)
        {
            _logger.LogInformation("[WhatsApp] Delivered to {Phone} for notification {Id}", phone, notification.Id);
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

        _logger.LogWarning("[WhatsApp] Failed to deliver to {Phone}: {Error}", phone, result.Error);
        return new NotificationResult
        {
            Success = false,
            NotificationId = notification.Id,
            ErrorMessage = result.Error
        };
    }

    public Task<bool> ValidateAsync(Notification notification, CancellationToken cancellationToken = default)
        => Task.FromResult(!string.IsNullOrWhiteSpace(GetPhoneNumber(notification)));

    // =============================================
    // Private Helpers
    // =============================================

    private static string? GetPhoneNumber(Notification notification)
    {
        if (notification.Data == null) return null;
        foreach (var key in new[] { "WhatsApp", "Phone", "phone", "Mobile", "mobile" })
        {
            if (notification.Data.TryGetValue(key, out var v) && !string.IsNullOrWhiteSpace(v))
                return v;
        }
        return null;
    }

    private static string BuildWhatsAppText(Notification notification)
    {
        // WhatsApp يدعم bold بـ *text* وإيطالي بـ _text_
        var title = string.IsNullOrWhiteSpace(notification.Title)
            ? ""
            : $"*{notification.Title}*\n\n";

        return $"{title}{notification.Message}";
    }
}
