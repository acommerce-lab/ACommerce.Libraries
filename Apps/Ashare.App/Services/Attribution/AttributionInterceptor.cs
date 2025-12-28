using Microsoft.Extensions.Logging;

namespace Ashare.App.Services.Attribution;

/// <summary>
/// Interceptor لإضافة بيانات الإسناد التسويقي لكل طلب HTTP
/// </summary>
public sealed class AttributionInterceptor : DelegatingHandler
{
    private readonly IAttributionCaptureService _attributionService;
    private readonly ILogger<AttributionInterceptor> _logger;

    // Header names for attribution data
    public const string FbcHeader = "X-Attribution-Fbc";
    public const string FbpHeader = "X-Attribution-Fbp";
    public const string GclidHeader = "X-Attribution-Gclid";
    public const string TtclidHeader = "X-Attribution-Ttclid";
    public const string UtmSourceHeader = "X-Attribution-Utm-Source";
    public const string UtmMediumHeader = "X-Attribution-Utm-Medium";
    public const string UtmCampaignHeader = "X-Attribution-Utm-Campaign";
    public const string UtmContentHeader = "X-Attribution-Utm-Content";
    public const string UtmTermHeader = "X-Attribution-Utm-Term";

    public AttributionInterceptor(
        IAttributionCaptureService attributionService,
        ILogger<AttributionInterceptor> logger)
    {
        _attributionService = attributionService;
        _logger = logger;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        try
        {
            // Get current attribution data
            var attribution = await _attributionService.GetAttributionAsync();
            var fbp = await _attributionService.GetOrCreateFbpAsync();

            // Add Fbp header (always present)
            if (!string.IsNullOrEmpty(fbp))
            {
                request.Headers.TryAddWithoutValidation(FbpHeader, fbp);
            }

            // Add attribution headers if available
            if (attribution != null)
            {
                if (!string.IsNullOrEmpty(attribution.Fbc))
                    request.Headers.TryAddWithoutValidation(FbcHeader, attribution.Fbc);

                if (!string.IsNullOrEmpty(attribution.Gclid))
                    request.Headers.TryAddWithoutValidation(GclidHeader, attribution.Gclid);

                if (!string.IsNullOrEmpty(attribution.Ttclid))
                    request.Headers.TryAddWithoutValidation(TtclidHeader, attribution.Ttclid);

                if (!string.IsNullOrEmpty(attribution.UtmSource))
                    request.Headers.TryAddWithoutValidation(UtmSourceHeader, attribution.UtmSource);

                if (!string.IsNullOrEmpty(attribution.UtmMedium))
                    request.Headers.TryAddWithoutValidation(UtmMediumHeader, attribution.UtmMedium);

                if (!string.IsNullOrEmpty(attribution.UtmCampaign))
                    request.Headers.TryAddWithoutValidation(UtmCampaignHeader, attribution.UtmCampaign);

                if (!string.IsNullOrEmpty(attribution.UtmContent))
                    request.Headers.TryAddWithoutValidation(UtmContentHeader, attribution.UtmContent);

                if (!string.IsNullOrEmpty(attribution.UtmTerm))
                    request.Headers.TryAddWithoutValidation(UtmTermHeader, attribution.UtmTerm);

                _logger.LogDebug("[Attribution] Headers added: Fbc={Fbc}, Source={Source}",
                    attribution.Fbc, attribution.UtmSource);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "[Attribution] Error adding headers");
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
