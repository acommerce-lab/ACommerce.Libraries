using ACommerce.Notifications.Channels.Sms.Options;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;

namespace ACommerce.Notifications.Channels.Sms.Gateways;

/// <summary>
/// بوابة SMS عامة عبر HTTP REST.
/// تدعم أي API يقبل POST مع body JSON.
///
/// مثال الإعداد (appsettings.json):
///   "Notifications:Sms:Http": {
///     "BaseUrl": "https://api.smsprovider.com/v1/messages",
///     "ApiKey": "your-api-key",
///     "PhoneFieldName": "to",
///     "MessageFieldName": "body",
///     "SenderFieldName": "from"
///   }
/// </summary>
public class HttpSmsGateway : ISmsGateway
{
    private readonly HttpClient _httpClient;
    private readonly SmsOptions _options;
    private readonly ILogger<HttpSmsGateway> _logger;

    public HttpSmsGateway(
        HttpClient httpClient,
        SmsOptions options,
        ILogger<HttpSmsGateway> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<SmsResult> SendAsync(
        string toNumber,
        string message,
        string? from = null,
        CancellationToken ct = default)
    {
        var cfg = _options.Http;

        try
        {
            var body = new Dictionary<string, string>
            {
                [cfg.PhoneFieldName] = toNumber,
                [cfg.MessageFieldName] = message,
                [cfg.SenderFieldName] = from ?? _options.SenderNumber
            };

            var json = JsonSerializer.Serialize(body);
            using var request = new HttpRequestMessage(HttpMethod.Post, cfg.BaseUrl)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };

            // API Key header
            if (!string.IsNullOrEmpty(cfg.ApiKey))
                request.Headers.TryAddWithoutValidation("Authorization", $"Bearer {cfg.ApiKey}");

            // custom headers
            foreach (var (key, value) in cfg.Headers)
                request.Headers.TryAddWithoutValidation(key, value);

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            cts.CancelAfter(cfg.Timeout);

            var response = await _httpClient.SendAsync(request, cts.Token);
            var raw = await response.Content.ReadAsStringAsync(ct);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("[SMS] Sent to {Number}, status: {Status}", toNumber, response.StatusCode);
                return new SmsResult(true, RawResponse: raw);
            }

            _logger.LogWarning("[SMS] Failed to send to {Number}: {Status} - {Body}", toNumber, response.StatusCode, raw);
            return new SmsResult(false, Error: $"HTTP {(int)response.StatusCode}: {raw}", RawResponse: raw);
        }
        catch (TaskCanceledException)
        {
            _logger.LogError("[SMS] Timeout sending to {Number}", toNumber);
            return new SmsResult(false, Error: "Request timed out");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[SMS] Error sending to {Number}", toNumber);
            return new SmsResult(false, Error: ex.Message);
        }
    }
}
