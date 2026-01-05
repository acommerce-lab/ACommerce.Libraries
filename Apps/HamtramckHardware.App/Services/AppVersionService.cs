using ACommerce.Templates.Customer.Services;

namespace HamtramckHardware.App.Services;

/// <summary>
/// MAUI implementation of app version service
/// </summary>
public class AppVersionService : IAppVersionService
{
    public string Version => AppInfo.Current.VersionString;
    public string Build => AppInfo.Current.BuildString;
    public int BuildNumber => int.TryParse(Build, out var n) ? n : 0;
    public string PackageName => AppInfo.Current.PackageName;
    public bool IsMobileApp => true;
}
