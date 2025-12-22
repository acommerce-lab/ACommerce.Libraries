using Ashare.Shared.Services;
using Microsoft.Maui.ApplicationModel;

namespace Ashare.App.Services;

/// <summary>
/// تنفيذ خدمة إصدار التطبيق لـ MAUI
/// يستخدم AppInfo.Current للحصول على معلومات الإصدار من المنصة
/// </summary>
public class AppVersionService : IAppVersionService
{
    /// <summary>
    /// رقم الإصدار (مثل "1.0.0")
    /// </summary>
    public string Version => AppInfo.Current.VersionString;

    /// <summary>
    /// رقم البناء كنص (مثل "1" أو "100")
    /// </summary>
    public string Build => AppInfo.Current.BuildString;

    /// <summary>
    /// رقم البناء كعدد صحيح
    /// </summary>
    public int BuildNumber
    {
        get
        {
            if (int.TryParse(Build, out var buildNum))
            {
                return buildNum;
            }
            return 0;
        }
    }

    /// <summary>
    /// اسم الحزمة (مثل "com.ashare.ashare")
    /// </summary>
    public string PackageName => AppInfo.Current.PackageName;

    /// <summary>
    /// هذا تطبيق موبايل MAUI
    /// </summary>
    public bool IsMobileApp => true;
}
