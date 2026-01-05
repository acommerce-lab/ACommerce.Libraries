using ACommerce.Marketplace.GCP.Services;

namespace ACommerce.Marketplace.GCP.Infrastructure;

/// <summary>
/// Middleware that tracks API request usage for GCP Marketplace billing.
/// This automatically records every API request as a billable event.
///
/// Usage:
/// Add to pipeline: app.UseMiddleware{UsageTrackingMiddleware}();
///
/// Note: Place after authentication middleware to capture user context.
/// </summary>
public class UsageTrackingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<UsageTrackingMiddleware> _logger;

    // Paths to exclude from usage tracking
    private static readonly HashSet<string> ExcludedPaths = new(StringComparer.OrdinalIgnoreCase)
    {
        "/health",
        "/health/ready",
        "/health/startup",
        "/healthz",
        "/swagger",
        "/swagger/v1/swagger.json",
        "/"
    };

    public UsageTrackingMiddleware(RequestDelegate next, ILogger<UsageTrackingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IUsageReportingService usageReporting)
    {
        var path = context.Request.Path.Value ?? "";

        // Skip health checks and swagger
        if (ShouldSkipTracking(path))
        {
            await _next(context);
            return;
        }

        // Execute request
        await _next(context);

        // Only count successful API requests
        if (context.Response.StatusCode >= 200 && context.Response.StatusCode < 400)
        {
            try
            {
                var metadata = new Dictionary<string, string>
                {
                    ["path"] = path,
                    ["method"] = context.Request.Method,
                    ["status_code"] = context.Response.StatusCode.ToString()
                };

                // Add user context if authenticated
                if (context.User.Identity?.IsAuthenticated == true)
                {
                    var userId = context.User.FindFirst("sub")?.Value
                        ?? context.User.FindFirst("id")?.Value;
                    if (!string.IsNullOrEmpty(userId))
                    {
                        metadata["user_id"] = userId;
                    }
                }

                await usageReporting.RecordUsageAsync(UsageMetricType.ApiRequests, 1, metadata);
            }
            catch (Exception ex)
            {
                // Don't fail requests due to usage tracking errors
                _logger.LogWarning(ex, "Failed to record API usage for {Path}", path);
            }
        }
    }

    private static bool ShouldSkipTracking(string path)
    {
        // Skip exact matches
        if (ExcludedPaths.Contains(path))
            return true;

        // Skip swagger paths
        if (path.StartsWith("/swagger", StringComparison.OrdinalIgnoreCase))
            return true;

        return false;
    }
}

/// <summary>
/// Extension methods for adding usage tracking to the pipeline
/// </summary>
public static class UsageTrackingMiddlewareExtensions
{
    /// <summary>
    /// Adds API usage tracking middleware for GCP Marketplace billing
    /// </summary>
    public static IApplicationBuilder UseUsageTracking(this IApplicationBuilder app)
    {
        return app.UseMiddleware<UsageTrackingMiddleware>();
    }
}
