using Microsoft.Extensions.Logging;

namespace ACommerce.Client.Versions;

/// <summary>
/// إعدادات خدمة التحقق من الإصدار
/// </summary>
public class VersionCheckOptions
{
    /// <summary>
    /// كود تطبيق الموبايل
    /// </summary>
    public string MobileAppCode { get; set; } = "app-mobile";

    /// <summary>
    /// كود تطبيق الويب
    /// </summary>
    public string WebAppCode { get; set; } = "app-web";

    /// <summary>
    /// هل يجب حظر التطبيق في حالة فشل الفحص؟
    /// </summary>
    public bool BlockOnCheckFailure { get; set; } = true;
}

/// <summary>
/// خدمة التحقق من إصدار التطبيق
/// </summary>
public class VersionCheckService
{
    private readonly VersionsClient _versionsClient;
    private readonly ILogger<VersionCheckService> _logger;
    private readonly VersionCheckOptions _options;

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
            // Runtime detection using .NET OperatingSystem class
            return OperatingSystem.IsAndroid() ||
                   OperatingSystem.IsIOS() ||
                   OperatingSystem.IsMacCatalyst();
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

    /// <summary>
    /// إنشاء خدمة التحقق من الإصدار
    /// </summary>
    /// <param name="versionsClient">عميل الإصدارات</param>
    /// <param name="logger">المسجل</param>
    /// <param name="options">الإعدادات</param>
    /// <param name="applicationCode">كود التطبيق (إذا لم يتم تحديده، يتم الكشف التلقائي)</param>
    public VersionCheckService(
        VersionsClient versionsClient,
        ILogger<VersionCheckService> logger,
        VersionCheckOptions? options = null,
        string? applicationCode = null)
    {
        _versionsClient = versionsClient;
        _logger = logger;
        _options = options ?? new VersionCheckOptions();

        // استخدام الكود الممرر، أو الكشف التلقائي
        ApplicationCode = applicationCode ?? (IsMobilePlatform ? _options.MobileAppCode : _options.WebAppCode);
        _logger.LogInformation("VersionCheckService initialized with ApplicationCode: {Code}", ApplicationCode);
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
            _logger.LogError(ex, "Failed to check version");
            Console.WriteLine($"[VersionCheckService] API call failed: {ex.Message}");

            if (_options.BlockOnCheckFailure)
            {
                // في حالة فشل الفحص، نحظر التطبيق لأسباب أمنية
                _logger.LogError("BLOCKING app for security - version check failed");
                Console.WriteLine("[VersionCheckService] ⛔ BLOCKING app - version check failed");

                LastCheckResult = new VersionCheckResult
                {
                    IsSupported = false,
                    UpdateAvailable = false,
                    IsForceUpdate = true, // إجبار التحديث = حظر
                    CurrentStatus = VersionStatus.Unsupported,
                    MessageAr = "تعذر التحقق من إصدار التطبيق. يرجى التأكد من اتصالك بالإنترنت والمحاولة مرة أخرى.",
                    MessageEn = "Unable to verify app version. Please check your internet connection and try again."
                };
            }
            else
            {
                // السماح بالاستخدام في حالة الفشل
                LastCheckResult = new VersionCheckResult
                {
                    IsSupported = true,
                    UpdateAvailable = false,
                    IsForceUpdate = false,
                    CurrentStatus = VersionStatus.Supported,
                    MessageAr = "تعذر التحقق من الإصدار",
                    MessageEn = "Could not verify version"
                };
            }

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
