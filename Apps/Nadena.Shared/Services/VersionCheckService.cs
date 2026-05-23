using ACommerce.Client.Versions;
using Microsoft.Extensions.Logging;

namespace Nadena.Shared.Services;

/// <summary>
/// خدمة التحقق من إصدار تطبيق نادينا
/// تستخدم VersionCheckService من المكتبة مع إعدادات نادينا
/// </summary>
public class VersionCheckService : ACommerce.Client.Versions.VersionCheckService
{
    /// <summary>
    /// كود تطبيق الموبايل لنادينا
    /// </summary>
    public new const string MobileAppCode = "ashare-mobile";

    /// <summary>
    /// كود تطبيق الويب لنادينا
    /// </summary>
    public new const string WebAppCode = "ashare-web";

    /// <summary>
    /// إعدادات نادينا الافتراضية
    /// </summary>
    public static VersionCheckOptions NadenaOptions => new()
    {
        MobileAppCode = MobileAppCode,
        WebAppCode = WebAppCode,
        BlockOnCheckFailure = true
    };

    public VersionCheckService(
        VersionsClient versionsClient,
        ILogger<VersionCheckService> logger,
        string? applicationCode = null)
        : base(versionsClient, logger, NadenaOptions, applicationCode)
    {
    }
}
