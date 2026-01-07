namespace Restaurant.Vendor.App.Services;

public class ApiSettings
{
#if DEBUG
    public static string BaseUrl => DeviceInfo.Platform == DevicePlatform.Android
        ? "https://10.0.2.2:5002"  // Android Emulator
        : "https://localhost:5002"; // iOS Simulator / Windows
#else
    public static string BaseUrl => "https://vendor-api.restaurant.example.com";
#endif

    public static Uri BaseUri => new(BaseUrl);

    public static void LogConfiguration()
    {
        Console.WriteLine($"[ApiSettings] Platform: {DeviceInfo.Platform}");
        Console.WriteLine($"[ApiSettings] BaseUrl: {BaseUrl}");
    }
}
