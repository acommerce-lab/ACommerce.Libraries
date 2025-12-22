using ACommerce.Client.Versions;
using Microsoft.Extensions.Logging;

namespace Ashare.Shared.Services;

/// <summary>
/// خدمة التحقق من إصدار التطبيق
/// </summary>
public class VersionCheckService
{
    private readonly VersionsClient _versionsClient;
    private readonly ILogger<VersionCheckService> _logger;

    /// <summary>
    /// كود تطبيق الموبايل
    /// </summary>
    public const string MobileAppCode = "ashare-mobile";

    /// <summary>
    /// كود تطبيق الويب
    /// </summary>
    public const string WebAppCode = "ashare-web";

    /// <summary>
    /// كود التطبيق الحالي (يتم تحديده حسب المنصة)
    /// </summary>
    public string ApplicationCode { get; }

    /// <summary>
    /// تحديد المنصة تلقائياً
    /// </summary>
    private static bool IsMobilePlatform
    {
        get
        {
#if ANDROID || IOS || MACCATALYST
            return true;
#else
            return false;
#endif
        }
    }

    /// <summary>
    /// نتيجة آخر فحص
    /// </summary>
    public VersionCheckResult? LastCheckResult { get; private set; }

    /// <summary>
    /// هل تم الفحص؟
    /// </summary>
    public bool HasChecked { get; private set; }

    /// <summary>
    /// هل الإصدار محظور (غير مدعوم)؟
    /// </summary>
    public bool IsBlocked => LastCheckResult?.IsSupported == false && LastCheckResult?.IsForceUpdate == true;

    /// <summary>
    /// هل يوجد تحذير (إصدار على وشك الانتهاء)؟
    /// </summary>
    public bool HasWarning => LastCheckResult?.CurrentStatus == VersionStatus.Deprecated;

    /// <summary>
    /// هل يوجد تحديث متاح؟
    /// </summary>
    public bool UpdateAvailable => LastCheckResult?.UpdateAvailable == true;

    public VersionCheckService(VersionsClient versionsClient, ILogger<VersionCheckService> logger)
    {
        _versionsClient = versionsClient;
        _logger = logger;
        ApplicationCode = IsMobilePlatform ? MobileAppCode : WebAppCode;
    }

    /// <summary>
    /// فحص الإصدار الحالي
    /// </summary>
    /// <param name="currentVersion">رقم الإصدار الحالي (مثل 1.14)</param>
    /// <param name="buildNumber">رقم البناء (اختياري)</param>
    /// <param name="cancellationToken">رمز الإلغاء</param>
    public async Task<VersionCheckResult?> CheckVersionAsync(
        string currentVersion,
        int? buildNumber = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Checking version {Version} (build {Build})", currentVersion, buildNumber);

            var request = new VersionCheckRequest
            {
                ApplicationCode = ApplicationCode,
                CurrentVersion = currentVersion,
                CurrentBuildNumber = buildNumber
            };

            LastCheckResult = await _versionsClient.CheckVersionAsync(request, cancellationToken);
            HasChecked = true;

            if (LastCheckResult != null)
            {
                LogVersionCheckResult(LastCheckResult);
            }

            return LastCheckResult;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to check version - assuming supported");
            // في حالة فشل الفحص، نفترض أن الإصدار مدعوم
            LastCheckResult = new VersionCheckResult
            {
                IsSupported = true,
                UpdateAvailable = false,
                IsForceUpdate = false,
                CurrentStatus = VersionStatus.Supported
            };
            HasChecked = true;
            return LastCheckResult;
        }
    }

    private void LogVersionCheckResult(VersionCheckResult result)
    {
        if (!result.IsSupported)
        {
            _logger.LogWarning("Version is NOT supported! Force update required: {ForceUpdate}", result.IsForceUpdate);
        }
        else if (result.CurrentStatus == VersionStatus.Deprecated)
        {
            _logger.LogWarning("Version is DEPRECATED. End of support: {EndDate}", result.EndOfSupportDate);
        }
        else if (result.UpdateAvailable)
        {
            _logger.LogInformation("Update available. Latest: {Latest}", result.LatestVersion?.VersionNumber);
        }
        else
        {
            _logger.LogInformation("Version is up to date (Status: {Status})", result.CurrentStatus);
        }
    }
}
