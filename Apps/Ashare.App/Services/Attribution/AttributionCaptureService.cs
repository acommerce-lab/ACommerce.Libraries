using System.Text.Json;
using System.Web;
using Microsoft.Extensions.Logging;

namespace Ashare.App.Services.Attribution;

/// <summary>
/// تنفيذ خدمة التقاط بيانات الإسناد التسويقي
/// </summary>
public class AttributionCaptureService : IAttributionCaptureService
{
    private const string AttributionStorageKey = "ashare_attribution_data";
    private const string FbpStorageKey = "ashare_fbp";

    private readonly ILogger<AttributionCaptureService> _logger;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public event EventHandler<AttributionData>? AttributionCaptured;

    public AttributionCaptureService(ILogger<AttributionCaptureService> logger)
    {
        _logger = logger;
    }

    public Task CaptureFromUrlAsync(string url)
    {
        if (Uri.TryCreate(url, UriKind.Absolute, out var uri))
        {
            return CaptureFromUrlAsync(uri);
        }

        _logger.LogWarning("[Attribution] Invalid URL: {Url}", url);
        return Task.CompletedTask;
    }

    public async Task CaptureFromUrlAsync(Uri url)
    {
        try
        {
            _logger.LogInformation("[Attribution] Capturing from URL: {Url}", url);

            var query = HttpUtility.ParseQueryString(url.Query);
            var attribution = new AttributionData
            {
                OriginalUrl = url.ToString(),
                CapturedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(7),

                // Facebook
                Fbclid = query["fbclid"],

                // Google
                Gclid = query["gclid"],

                // TikTok
                Ttclid = query["ttclid"],

                // UTM Parameters
                UtmSource = query["utm_source"],
                UtmMedium = query["utm_medium"],
                UtmCampaign = query["utm_campaign"],
                UtmContent = query["utm_content"],
                UtmTerm = query["utm_term"]
            };

            // Generate _fbc if fbclid exists
            if (!string.IsNullOrEmpty(attribution.Fbclid))
            {
                var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                attribution.Fbc = $"fb.1.{timestamp}.{attribution.Fbclid}";
            }

            // Get or create _fbp
            attribution.Fbp = await GetOrCreateFbpAsync();

            if (attribution.HasAttribution)
            {
                await SaveAttributionAsync(attribution);
                _logger.LogInformation("[Attribution] ✅ Captured: Source={Source}, Campaign={Campaign}, Fbclid={Fbclid}",
                    attribution.UtmSource, attribution.UtmCampaign, attribution.Fbclid);

                AttributionCaptured?.Invoke(this, attribution);
            }
            else
            {
                _logger.LogDebug("[Attribution] No attribution data found in URL");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Attribution] Error capturing from URL");
        }
    }

    public async Task<AttributionData?> GetAttributionAsync()
    {
        try
        {
            var json = await SecureStorage.Default.GetAsync(AttributionStorageKey);
            if (string.IsNullOrEmpty(json))
            {
                return null;
            }

            var attribution = JsonSerializer.Deserialize<AttributionData>(json, _jsonOptions);

            // Check if expired
            if (attribution?.IsExpired == true)
            {
                _logger.LogDebug("[Attribution] Data expired, clearing...");
                await ClearAttributionAsync();
                return null;
            }

            return attribution;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Attribution] Error reading attribution data");
            return null;
        }
    }

    public async Task ClearAttributionAsync()
    {
        try
        {
            SecureStorage.Default.Remove(AttributionStorageKey);
            _logger.LogDebug("[Attribution] Data cleared");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Attribution] Error clearing attribution data");
        }

        await Task.CompletedTask;
    }

    public async Task<string> GetOrCreateFbpAsync()
    {
        try
        {
            var fbp = await SecureStorage.Default.GetAsync(FbpStorageKey);

            if (string.IsNullOrEmpty(fbp))
            {
                // Generate new _fbp: fb.1.{timestamp}.{random}
                var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                var random = new Random().Next(1000000000, int.MaxValue);
                fbp = $"fb.1.{timestamp}.{random}";

                await SecureStorage.Default.SetAsync(FbpStorageKey, fbp);
                _logger.LogDebug("[Attribution] Generated new _fbp: {Fbp}", fbp);
            }

            return fbp;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Attribution] Error getting/creating _fbp");
            // Return a temporary one if storage fails
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            return $"fb.1.{timestamp}.{new Random().Next(1000000000, int.MaxValue)}";
        }
    }

    public async Task<string?> GetFbcAsync()
    {
        var attribution = await GetAttributionAsync();
        return attribution?.Fbc;
    }

    private async Task SaveAttributionAsync(AttributionData attribution)
    {
        try
        {
            var json = JsonSerializer.Serialize(attribution, _jsonOptions);
            await SecureStorage.Default.SetAsync(AttributionStorageKey, json);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Attribution] Error saving attribution data");
        }
    }
}
