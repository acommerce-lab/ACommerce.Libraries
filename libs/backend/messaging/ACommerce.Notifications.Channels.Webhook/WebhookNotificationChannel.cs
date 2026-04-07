using ACommerce.Notifications.Abstractions.Contracts;
using ACommerce.Notifications.Abstractions.Enums;
using ACommerce.Notifications.Abstractions.Models;
using ACommerce.Notifications.Channels.Webhook.Options;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace ACommerce.Notifications.Channels.Webhook;

/// <summary>
/// قناة إشعارات Webhook.
/// ترسل الإشعار عبر HTTP POST لـ URL مُعرّف في الإعدادات أو في Notification.Data.
///
/// Notification.Data المدعومة:
///   "WebhookUrl" → URL مخصص لهذا الإشعار (يتجاوز الافتراضي)
/// </summary>
public class WebhookNotificationChannel : INotificationChannel
{
    private readonly HttpClient _httpClient;
    private readonly WebhookOptions _options;
    private readonly ILogger<WebhookNotificationChannel> _logger;

    public NotificationChannel Channel => NotificationChannel.Webhook;

    public WebhookNotificationChannel(
        HttpClient httpClient,
        WebhookOptions options,
        ILogger<WebhookNotificationChannel> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<NotificationResult> SendAsync(
        Notification notification,
        CancellationToken cancellationToken = default)
    {
        var url = GetWebhookUrl(notification);

        if (string.IsNullOrWhiteSpace(url))
        {
            _logger.LogWarning("[Webhook] No URL for notification {Id}", notification.Id);
            return new NotificationResult
            {
                Success = false,
                NotificationId = notification.Id,
                ErrorMessage = "Webhook URL not configured. Set DefaultUrl in options or 'WebhookUrl' in Notification.Data."
            };
        }

        try
        {
            var payload = BuildPayload(notification);
            var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            using var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };

            // إضافة headers الإضافية
            foreach (var (key, value) in _options.Headers)
                request.Headers.TryAddWithoutValidation(key, value);

            // HMAC توقيع
            if (!string.IsNullOrEmpty(_options.HmacSecret))
            {
                var signature = ComputeHmacSignature(json, _options.HmacSecret);
                request.Headers.TryAddWithoutValidation(_options.SignatureHeaderName, $"sha256={signature}");
            }

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(_options.Timeout);

            _logger.LogDebug("[Webhook] POST to {Url} for notification {Id}", url, notification.Id);

            var response = await _httpClient.SendAsync(request, cts.Token);
            var raw = await response.Content.ReadAsStringAsync(cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("[Webhook] Delivered notification {Id} to {Url}", notification.Id, url);
                return new NotificationResult
                {
                    Success = true,
                    NotificationId = notification.Id,
                    SentAt = DateTimeOffset.UtcNow,
                    Metadata = new Dictionary<string, object>
                    {
                        ["statusCode"] = (int)response.StatusCode,
                        ["url"] = url
                    }
                };
            }

            _logger.LogWarning("[Webhook] Failed: {Status} from {Url} - {Body}", response.StatusCode, url, raw);
            return new NotificationResult
            {
                Success = false,
                NotificationId = notification.Id,
                ErrorMessage = $"HTTP {(int)response.StatusCode}: {raw}"
            };
        }
        catch (TaskCanceledException)
        {
            _logger.LogError("[Webhook] Timeout for notification {Id} to {Url}", notification.Id, url);
            return new NotificationResult
            {
                Success = false,
                NotificationId = notification.Id,
                ErrorMessage = "Webhook request timed out"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Webhook] Error sending notification {Id} to {Url}", notification.Id, url);
            return new NotificationResult
            {
                Success = false,
                NotificationId = notification.Id,
                ErrorMessage = ex.Message
            };
        }
    }

    public Task<bool> ValidateAsync(Notification notification, CancellationToken cancellationToken = default)
        => Task.FromResult(!string.IsNullOrWhiteSpace(GetWebhookUrl(notification)));

    // =============================================
    // Private Helpers
    // =============================================

    private string? GetWebhookUrl(Notification notification)
    {
        // أولوية: URL في بيانات الإشعار ثم الافتراضي
        if (notification.Data != null &&
            notification.Data.TryGetValue("WebhookUrl", out var customUrl) &&
            !string.IsNullOrWhiteSpace(customUrl))
        {
            return customUrl;
        }

        return string.IsNullOrWhiteSpace(_options.DefaultUrl) ? null : _options.DefaultUrl;
    }

    private object BuildPayload(Notification notification)
    {
        if (_options.PayloadFormat == WebhookPayloadFormat.Minimal)
        {
            return new
            {
                id = notification.Id,
                userId = notification.UserId,
                title = notification.Title,
                message = notification.Message,
                type = notification.Type.ToString(),
                timestamp = notification.CreatedAt
            };
        }

        // Full format
        return new
        {
            id = notification.Id,
            userId = notification.UserId,
            title = notification.Title,
            message = notification.Message,
            type = notification.Type.ToString(),
            priority = notification.Priority.ToString(),
            actionUrl = notification.ActionUrl,
            imageUrl = notification.ImageUrl,
            data = notification.Data,
            createdAt = notification.CreatedAt,
            expiresAt = notification.ExpiresAt
        };
    }

    private static string ComputeHmacSignature(string payload, string secret)
    {
        var keyBytes = Encoding.UTF8.GetBytes(secret);
        var payloadBytes = Encoding.UTF8.GetBytes(payload);

        using var hmac = new HMACSHA256(keyBytes);
        var hashBytes = hmac.ComputeHash(payloadBytes);
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }
}
