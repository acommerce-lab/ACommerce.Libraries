using ACommerce.Templates.Customer.Services;

namespace HamtramckHardware.Web.Services;

/// <summary>
/// Web implementation of app version service
/// </summary>
public class WebAppVersionService : IAppVersionService
{
    private const string _version = "1.0.0";
    private const string _build = "1";

    public string Version => _version;
    public string Build => _build;
    public int BuildNumber => int.TryParse(_build, out var n) ? n : 0;
    public string PackageName => "com.hamtramckhardware.web";
    public bool IsMobileApp => false;
}
