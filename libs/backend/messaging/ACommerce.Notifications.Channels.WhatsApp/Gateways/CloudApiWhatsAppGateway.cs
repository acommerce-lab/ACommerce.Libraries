using ACommerce.Notifications.Channels.WhatsApp.Options;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;

namespace ACommerce.Notifications.Channels.WhatsApp.Gateways;

/// <summary>
/// بوابة Meta WhatsApp Cloud API.
/// تدعم الرسائل النصية والقوالب (Templates).
///
/// الإعداد (appsettings.json):
///   "Notifications:WhatsApp:CloudApi": {
///     "AccessToken": "EAAx...",
///     "PhoneNumberId": "123456789"
///   }
/// </summary>
public class CloudApiWhatsAppGateway : IWhatsAppGateway
{
    private readonly HttpClient _httpClient;
    private readonly WhatsAppOptions _options;
    private readonly ILogger<CloudApiWhatsAppGateway> _logger;

    public CloudApiWhatsAppGateway(
        HttpClient httpClient,
        WhatsAppOptions options,
        ILogger<CloudApiWhatsAppGateway> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<WhatsAppResult> SendTextAsync(
        string toNumber,
        string message,
        CancellationToken ct = default)
    {
        var cfg = _options.CloudApi;
        var url = cfg.GetSendUrl();

        var payload = new
        {
            messaging_product = "whatsapp",
            recipient_type = "individual",
            to = NormalizeNumber(toNumber),
            type = "text",
            text = new { preview_url = false, body = message }
        };

        return await SendRequestAsync(url, cfg.AccessToken, payload, ct);
    }

    public async Task<WhatsAppResult> SendTemplateAsync(
        string toNumber,
        string templateName,
        string languageCode,
        IEnumerable<string>? parameters = null,
        CancellationToken ct = default)
    {
        var cfg = _options.CloudApi;
        var url = cfg.GetSendUrl();

        object template;

        if (parameters != null)
        {
            var components = new[]
            {
                new
                {
                    type = "body",
                    parameters = parameters
                        .Select(p => new { type = "text", text = p })
                        .ToArray()
                }
            };

            template = new
            {
                messaging_product = "whatsapp",
                to = NormalizeNumber(toNumber),
                type = "template",
                template = new
                {
                    name = templateName,
                    language = new { code = languageCode },
                    components
                }
            };
        }
        else
        {
            template = new
            {
                messaging_product = "whatsapp",
                to = NormalizeNumber(toNumber),
                type = "template",
                template = new
                {
                    name = templateName,
                    language = new { code = languageCode }
                }
            };
        }

        return await SendRequestAsync(url, cfg.AccessToken, template, ct);
    }

    private async Task<WhatsAppResult> SendRequestAsync(
        string url,
        string accessToken,
        object payload,
        CancellationToken ct)
    {
        try
        {
            var json = JsonSerializer.Serialize(payload);
            using var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
            request.Headers.TryAddWithoutValidation("Authorization", $"Bearer {accessToken}");

            var response = await _httpClient.SendAsync(request, ct);
            var raw = await response.Content.ReadAsStringAsync(ct);

            if (response.IsSuccessStatusCode)
            {
                // استخراج message id من الرد
                string? messageId = null;
                try
                {
                    using var doc = JsonDocument.Parse(raw);
                    if (doc.RootElement.TryGetProperty("messages", out var msgs) &&
                        msgs.ValueKind == JsonValueKind.Array &&
                        msgs.GetArrayLength() > 0)
                    {
                        messageId = msgs[0].GetProperty("id").GetString();
                    }
                }
                catch { /* ignore parse errors */ }

                _logger.LogInformation("[WhatsApp] Sent to {Number}, msgId: {Id}", url, messageId);
                return new WhatsAppResult(true, MessageId: messageId, RawResponse: raw);
            }

            _logger.LogWarning("[WhatsApp] Failed: {Status} - {Body}", response.StatusCode, raw);
            return new WhatsAppResult(false, Error: $"HTTP {(int)response.StatusCode}: {raw}", RawResponse: raw);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[WhatsApp] Error sending message");
            return new WhatsAppResult(false, Error: ex.Message);
        }
    }

    /// <summary>
    /// تحويل الرقم لصيغة دولية بدون + (متطلب WhatsApp Cloud API)
    /// مثال: +966501234567 → 966501234567
    /// </summary>
    private static string NormalizeNumber(string number)
        => number.TrimStart('+').Replace(" ", "").Replace("-", "");
}
