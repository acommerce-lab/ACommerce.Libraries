using HamtramckApiConfig = HamtramckHardware.Shared.Services.ApiConfiguration;

namespace HamtramckHardware.App.Services;

/// <summary>
/// API configuration for the MAUI app.
/// Uses environment-specific URLs for development and production.
/// </summary>
public static class ApiSettings
{
    /// <summary>
    /// Gets the base URL for the API.
    /// </summary>
    public static string BaseUrl
    {
        get
        {
#if DEBUG
            // Use localhost tunnel or production URL based on preference
            // For local development with tunnel:
            // return "https://your-local-tunnel.ngrok.io";

            // For production API (default for development):
            return HamtramckApiConfig.DefaultProductionUrl;
#else
            return HamtramckApiConfig.DefaultProductionUrl;
#endif
        }
    }

    /// <summary>
    /// Gets the base URI for the API.
    /// </summary>
    public static Uri BaseUri => new(BaseUrl);

    /// <summary>
    /// Logs the current API configuration for debugging purposes.
    /// </summary>
    public static void LogConfiguration()
    {
        Console.WriteLine("=== Hamtramck Hardware API Configuration ===");
        Console.WriteLine($"  BaseUrl: {BaseUrl}");
#if DEBUG
        Console.WriteLine("  Mode: DEBUG");
#else
        Console.WriteLine("  Mode: RELEASE");
#endif
        Console.WriteLine("=========================================");
    }
}
