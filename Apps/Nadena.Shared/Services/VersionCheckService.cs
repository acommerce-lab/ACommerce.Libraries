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
    public new const string MobileAppCode = "nadena-mobile";

    /// <summary>
    /// كود تطبيق الويب لنادينا
    /// </summary>
    public new const string WebAppCode = "nadena-web";

    /// <summary>
    /// إعدادات نادينا الافتراضية
    /// </summary>
    public static VersionCheckOptions NadenaOptions => new()
    {
        MobileAppCode = MobileAppCode,
        WebAppCode = WebAppCode,
        // لا تحظر التطبيق إذا فشل فحص الإصدار (مثلاً مهلة الشبكة على خادم بطيء) — مجرد تحذير
        BlockOnCheckFailure = false
    };

    public VersionCheckService(
        VersionsClient versionsClient,
        ILogger<VersionCheckService> logger,
        string? applicationCode = null)
        : base(versionsClient, logger, NadenaOptions, applicationCode)
    {
    }
}
