using ACommerce.Client.Versions;
using Microsoft.Extensions.Logging;

namespace Ashare.Shared.Services;

/// <summary>
/// خدمة التحقق من إصدار تطبيق عشير
/// تستخدم VersionCheckService من المكتبة مع إعدادات عشير
/// </summary>
public class VersionCheckService : ACommerce.Client.Versions.VersionCheckService
{
    /// <summary>
    /// كود تطبيق الموبايل لعشير
    /// </summary>
    public new const string MobileAppCode = "ashare-mobile";

    /// <summary>
    /// كود تطبيق الويب لعشير
    /// </summary>
    public new const string WebAppCode = "ashare-web";

    /// <summary>
    /// إعدادات عشير الافتراضية
    /// </summary>
    public static VersionCheckOptions AshareOptions => new()
    {
        MobileAppCode = MobileAppCode,
        WebAppCode = WebAppCode,
        BlockOnCheckFailure = true
    };

    public VersionCheckService(
        VersionsClient versionsClient,
        ILogger<VersionCheckService> logger,
        string? applicationCode = null)
        : base(versionsClient, logger, AshareOptions, applicationCode)
    {
    }
}
