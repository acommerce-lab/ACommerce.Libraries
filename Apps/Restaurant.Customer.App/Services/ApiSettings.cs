namespace Restaurant.Customer.App.Services;

public class ApiSettings
{
#if DEBUG
    public static string BaseUrl => DeviceInfo.Platform == DevicePlatform.Android
        ? "https://10.0.2.2:5001"  // Android Emulator
        : "https://localhost:5001"; // iOS Simulator / Windows
#else
    public static string BaseUrl => "https://api.restaurant.example.com";
#endif

    public static Uri BaseUri => new(BaseUrl);

    public static void LogConfiguration()
    {
        Console.WriteLine($"[ApiSettings] Platform: {DeviceInfo.Platform}");
        Console.WriteLine($"[ApiSettings] BaseUrl: {BaseUrl}");
    }
}
