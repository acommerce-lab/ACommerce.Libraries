using Microsoft.AspNetCore.Http;

namespace ACommerce.Marketing.Analytics.Services;

/// <summary>
/// Helper class to extract attribution data from HTTP headers
/// Mobile apps send attribution data via custom headers
/// </summary>
public static class AttributionHeaderReader
{
    // Header names (must match AttributionInterceptor in mobile app)
    public const string FbcHeader = "X-Attribution-Fbc";
    public const string FbpHeader = "X-Attribution-Fbp";
    public const string GclidHeader = "X-Attribution-Gclid";
    public const string TtclidHeader = "X-Attribution-Ttclid";
    public const string UtmSourceHeader = "X-Attribution-Utm-Source";
    public const string UtmMediumHeader = "X-Attribution-Utm-Medium";
    public const string UtmCampaignHeader = "X-Attribution-Utm-Campaign";
    public const string UtmContentHeader = "X-Attribution-Utm-Content";
    public const string UtmTermHeader = "X-Attribution-Utm-Term";

    /// <summary>
    /// Extract attribution data from HTTP request headers and populate UserTrackingContext
    /// </summary>
    public static void PopulateFromHeaders(UserTrackingContext context, HttpContext httpContext)
    {
        if (httpContext?.Request?.Headers == null)
            return;

        var headers = httpContext.Request.Headers;

        // Facebook Click ID (_fbc)
        if (headers.TryGetValue(FbcHeader, out var fbc) && !string.IsNullOrEmpty(fbc))
            context.Fbc = fbc.ToString();

        // Facebook Browser ID (_fbp)
        if (headers.TryGetValue(FbpHeader, out var fbp) && !string.IsNullOrEmpty(fbp))
            context.Fbp = fbp.ToString();

        // Also check cookies for web clients
        if (string.IsNullOrEmpty(context.Fbc))
        {
            var fbcCookie = httpContext.Request.Cookies["_fbc"];
            if (!string.IsNullOrEmpty(fbcCookie))
                context.Fbc = fbcCookie;
        }

        if (string.IsNullOrEmpty(context.Fbp))
        {
            var fbpCookie = httpContext.Request.Cookies["_fbp"];
            if (!string.IsNullOrEmpty(fbpCookie))
                context.Fbp = fbpCookie;
        }
    }

    /// <summary>
    /// Create a UserTrackingContext from HTTP request headers
    /// </summary>
    public static UserTrackingContext CreateFromRequest(HttpContext httpContext, string? userId = null)
    {
        var context = new UserTrackingContext
        {
            UserId = userId,
            IpAddress = GetClientIpAddress(httpContext),
            UserAgent = httpContext?.Request?.Headers.UserAgent.ToString()
        };

        PopulateFromHeaders(context, httpContext!);

        return context;
    }

    /// <summary>
    /// Get client IP address, considering proxies
    /// </summary>
    private static string? GetClientIpAddress(HttpContext? httpContext)
    {
        if (httpContext == null)
            return null;

        // Check for forwarded headers (behind proxy/load balancer)
        var forwardedFor = httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            // Take the first IP in the chain (original client)
            var ips = forwardedFor.Split(',', StringSplitOptions.RemoveEmptyEntries);
            if (ips.Length > 0)
                return ips[0].Trim();
        }

        var realIp = httpContext.Request.Headers["X-Real-IP"].FirstOrDefault();
        if (!string.IsNullOrEmpty(realIp))
            return realIp;

        return httpContext.Connection.RemoteIpAddress?.ToString();
    }
}
