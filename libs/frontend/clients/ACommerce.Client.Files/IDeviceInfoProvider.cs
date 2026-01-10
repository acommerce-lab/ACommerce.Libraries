namespace ACommerce.Client.Files;

/// <summary>
/// واجهة لتوفير معلومات الجهاز للتطبيقات
/// يتم تنفيذها في تطبيق MAUI للحصول على المعلومات الحقيقية
/// </summary>
public interface IDeviceInfoProvider
{
    /// <summary>
    /// المنصة (Android, iOS, Windows, etc.)
    /// </summary>
    string Platform { get; }

    /// <summary>
    /// إصدار التطبيق
    /// </summary>
    string AppVersion { get; }

    /// <summary>
    /// إصدار نظام التشغيل
    /// </summary>
    string OsVersion { get; }

    /// <summary>
    /// طراز الجهاز
    /// </summary>
    string DeviceModel { get; }

    /// <summary>
    /// الشركة المصنعة
    /// </summary>
    string Manufacturer { get; }

    /// <summary>
    /// نوع الاتصال (WiFi, Cellular, etc.)
    /// </summary>
    string NetworkType { get; }
}

/// <summary>
/// تنفيذ افتراضي عندما لا يكون هناك provider محدد
/// </summary>
public class DefaultDeviceInfoProvider : IDeviceInfoProvider
{
    public string Platform => GetPlatformFromEnvironment();
    public string AppVersion => "Unknown";
    public string OsVersion => Environment.OSVersion.ToString();
    public string DeviceModel => "Unknown";
    public string Manufacturer => "Unknown";
    public string NetworkType => "Unknown";

    private static string GetPlatformFromEnvironment()
    {
        if (OperatingSystem.IsAndroid()) return "Android";
        if (OperatingSystem.IsIOS()) return "iOS";
        if (OperatingSystem.IsWindows()) return "Windows";
        if (OperatingSystem.IsMacOS() || OperatingSystem.IsMacCatalyst()) return "MacOS";
        return "Unknown";
    }
}
