using System.Reflection;
using Ashare.Shared.Services;

namespace Ashare.Web.Services;

/// <summary>
/// تنفيذ خدمة إصدار التطبيق للويب
/// يستخدم معلومات Assembly للحصول على الإصدار
/// </summary>
public class WebAppVersionService : IAppVersionService
{
    private readonly string _version;
    private readonly string _build;
    private readonly int _buildNumber;

    public WebAppVersionService()
    {
        var assembly = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
        var version = assembly.GetName().Version ?? new Version(1, 0, 0, 0);

        _version = $"{version.Major}.{version.Minor}.{version.Build}";
        _build = version.Revision.ToString();
        _buildNumber = version.Revision;
    }

    /// <summary>
    /// رقم الإصدار (مثل "1.0.0")
    /// </summary>
    public string Version => _version;

    /// <summary>
    /// رقم البناء
    /// </summary>
    public string Build => _build;

    /// <summary>
    /// رقم البناء كعدد صحيح
    /// </summary>
    public int BuildNumber => _buildNumber;

    /// <summary>
    /// اسم الحزمة - للويب نستخدم اسم التطبيق
    /// </summary>
    public string PackageName => "sa.ashare.web";

    /// <summary>
    /// هذا ليس تطبيق موبايل
    /// </summary>
    public bool IsMobileApp => false;
}
