using ACommerce.Client.Versions;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

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
    /// تحديد المنصة تلقائياً باستخدام Runtime Detection
    /// </summary>
    private static bool IsMobilePlatform
    {
        get
        {
            // Runtime detection - works in shared projects
            var platform = DeviceInfo.Current.Platform;
            return platform == DevicePlatform.Android ||
                   platform == DevicePlatform.iOS ||
                   platform == DevicePlatform.MacCatalyst;
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
    /// هل الإصدار محظور؟
    /// يتم الحظر إذا كانت الحالة "غير مدعوم" أو إذا كان التحديث إجبارياً
    /// </summary>
    public bool IsBlocked =>
        LastCheckResult?.CurrentStatus == VersionStatus.Unsupported ||
        LastCheckResult?.IsForceUpdate == true;

    /// <summary>
    /// هل يوجد تحذير (إصدار على وشك الانتهاء)؟
    /// يظهر فقط للإصدارات Deprecated وعندما لا يكون محظوراً
    /// </summary>
    public bool HasWarning =>
        !IsBlocked && LastCheckResult?.CurrentStatus == VersionStatus.Deprecated;

    /// <summary>
    /// هل يوجد تحديث متاح؟
    /// يظهر فقط للإصدارات المدعومة عندما يكون هناك إصدار أحدث
    /// </summary>
    public bool UpdateAvailable =>
        !IsBlocked && !HasWarning && LastCheckResult?.UpdateAvailable == true;

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
            //Debug.WriteLine("Checking version {Version} (build {Build})", currentVersion, buildNumber);
            //Console.WriteLine("Checking version {Version} (build {Build})", currentVersion, buildNumber);

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
            _logger.LogError(ex, "Failed to check version - BLOCKING app for security");
            Console.WriteLine($"[VersionCheckService] ⛔ API call failed: {ex.Message} - BLOCKING app");

            // في حالة فشل الفحص، نحظر التطبيق لأسباب أمنية
            // يجب أن يتم التحقق من الإصدار بنجاح للسماح بالاستخدام
            LastCheckResult = new VersionCheckResult
            {
                IsSupported = false,
                UpdateAvailable = false,
                IsForceUpdate = true, // إجبار التحديث = حظر
                CurrentStatus = VersionStatus.Unsupported,
                MessageAr = "تعذر التحقق من إصدار التطبيق. يرجى التأكد من اتصالك بالإنترنت والمحاولة مرة أخرى.",
                MessageEn = "Unable to verify app version. Please check your internet connection and try again."
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
