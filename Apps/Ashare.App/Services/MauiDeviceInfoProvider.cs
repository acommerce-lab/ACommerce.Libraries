using ACommerce.Client.Files;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Networking;

namespace Ashare.App.Services;

/// <summary>
/// تنفيذ MAUI لتوفير معلومات الجهاز
/// يستخدم MAUI APIs للحصول على المعلومات الحقيقية
/// </summary>
public class MauiDeviceInfoProvider : IDeviceInfoProvider
{
    public string Platform
    {
        get
        {
            try
            {
                var platform = DeviceInfo.Platform;
                if (platform == DevicePlatform.Android) return "Android";
                if (platform == DevicePlatform.iOS) return "iOS";
                if (platform == DevicePlatform.WinUI) return "Windows";
                if (platform == DevicePlatform.MacCatalyst) return "MacOS";
                return platform.ToString();
            }
            catch
            {
                return "Unknown";
            }
        }
    }

    public string AppVersion
    {
        get
        {
            try
            {
                return AppInfo.VersionString;
            }
            catch
            {
                return "Unknown";
            }
        }
    }

    public string OsVersion
    {
        get
        {
            try
            {
                return DeviceInfo.VersionString;
            }
            catch
            {
                return "Unknown";
            }
        }
    }

    public string DeviceModel
    {
        get
        {
            try
            {
                return DeviceInfo.Model;
            }
            catch
            {
                return "Unknown";
            }
        }
    }

    public string Manufacturer
    {
        get
        {
            try
            {
                return DeviceInfo.Manufacturer;
            }
            catch
            {
                return "Unknown";
            }
        }
    }

    public string NetworkType
    {
        get
        {
            try
            {
                var profiles = Connectivity.Current.ConnectionProfiles;
                if (profiles.Contains(ConnectionProfile.WiFi)) return "WiFi";
                if (profiles.Contains(ConnectionProfile.Cellular)) return "Cellular";
                if (profiles.Contains(ConnectionProfile.Ethernet)) return "Ethernet";
                return Connectivity.Current.NetworkAccess.ToString();
            }
            catch
            {
                return "Unknown";
            }
        }
    }
}
