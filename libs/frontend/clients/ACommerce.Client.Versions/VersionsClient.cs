using System.Text.Json.Serialization;
using ACommerce.Client.Core.Http;

namespace ACommerce.Client.Versions;

/// <summary>
/// عميل إدارة الإصدارات
/// </summary>
public sealed class VersionsClient(IApiClient httpClient)
{
    private const string ServiceName = "Marketplace";

    #region Public Methods (للعميل)

    /// <summary>
    /// فحص الإصدار الحالي
    /// </summary>
    public async Task<VersionCheckResult?> CheckVersionAsync(
        VersionCheckRequest request,
        CancellationToken cancellationToken = default)
    {
        return await httpClient.PostAsync<VersionCheckRequest, VersionCheckResult>(
            ServiceName, "/api/versions/check", request, cancellationToken);
    }

    /// <summary>
    /// الحصول على أحدث إصدار لتطبيق
    /// </summary>
    public async Task<AppVersionDto?> GetLatestAsync(
        string applicationCode,
        CancellationToken cancellationToken = default)
    {
        return await httpClient.GetAsync<AppVersionDto>(
            ServiceName, $"/api/versions/latest/{applicationCode}", cancellationToken);
    }

    /// <summary>
    /// الحصول على إصدار معين
    /// </summary>
    public async Task<AppVersionDto?> GetByVersionAsync(
        string applicationCode,
        string versionNumber,
        CancellationToken cancellationToken = default)
    {
        return await httpClient.GetAsync<AppVersionDto>(
            ServiceName, $"/api/versions/{applicationCode}/{versionNumber}", cancellationToken);
    }

    #endregion

    #region Admin Methods (للإدارة)

    /// <summary>
    /// الحصول على جميع الإصدارات
    /// </summary>
    public async Task<IEnumerable<AppVersionDto>?> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await httpClient.GetAsync<IEnumerable<AppVersionDto>>(
            ServiceName, "/api/versions", cancellationToken);
    }

    /// <summary>
    /// الحصول على الإصدارات النشطة
    /// </summary>
    public async Task<IEnumerable<AppVersionDto>?> GetActiveAsync(
        CancellationToken cancellationToken = default)
    {
        return await httpClient.GetAsync<IEnumerable<AppVersionDto>>(
            ServiceName, "/api/versions/active", cancellationToken);
    }

    /// <summary>
    /// الحصول على إصدار بواسطة المعرف
    /// </summary>
    public async Task<AppVersionDto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await httpClient.GetAsync<AppVersionDto>(
            ServiceName, $"/api/versions/{id}", cancellationToken);
    }

    /// <summary>
    /// الحصول على إصدارات تطبيق معين
    /// </summary>
    public async Task<IEnumerable<AppVersionDto>?> GetByApplicationAsync(
        string applicationCode,
        CancellationToken cancellationToken = default)
    {
        return await httpClient.GetAsync<IEnumerable<AppVersionDto>>(
            ServiceName, $"/api/versions/app/{applicationCode}", cancellationToken);
    }

    /// <summary>
    /// الحصول على قائمة التطبيقات
    /// </summary>
    public async Task<IEnumerable<string>?> GetApplicationsAsync(
        CancellationToken cancellationToken = default)
    {
        return await httpClient.GetAsync<IEnumerable<string>>(
            ServiceName, "/api/versions/applications", cancellationToken);
    }

    /// <summary>
    /// إنشاء إصدار جديد
    /// </summary>
    public async Task<AppVersionDto?> CreateAsync(
        CreateAppVersionRequest request,
        CancellationToken cancellationToken = default)
    {
        return await httpClient.PostAsync<CreateAppVersionRequest, AppVersionDto>(
            ServiceName, "/api/versions", request, cancellationToken);
    }

    /// <summary>
    /// تحديث إصدار
    /// </summary>
    public async Task<AppVersionDto?> UpdateAsync(
        Guid id,
        UpdateAppVersionRequest request,
        CancellationToken cancellationToken = default)
    {
        return await httpClient.PutAsync<UpdateAppVersionRequest, AppVersionDto>(
            ServiceName, $"/api/versions/{id}", request, cancellationToken);
    }

    /// <summary>
    /// تغيير حالة الإصدار
    /// </summary>
    public async Task<AppVersionDto?> ChangeStatusAsync(
        Guid id,
        VersionStatus newStatus,
        CancellationToken cancellationToken = default)
    {
        return await httpClient.PatchAsync<VersionStatus, AppVersionDto>(
            ServiceName, $"/api/versions/{id}/status", newStatus, cancellationToken);
    }

    /// <summary>
    /// تفعيل/تعطيل إصدار
    /// </summary>
    public async Task<AppVersionDto?> ToggleActiveAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await httpClient.PatchAsync<object, AppVersionDto>(
            ServiceName, $"/api/versions/{id}/toggle-active", new { }, cancellationToken);
    }

    /// <summary>
    /// حذف إصدار
    /// </summary>
    public async Task DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        await httpClient.DeleteAsync(ServiceName, $"/api/versions/{id}", cancellationToken);
    }

    #endregion
}

#region DTOs

/// <summary>
/// حالة دعم الإصدار
/// </summary>
public enum VersionStatus
{
    Development = 0,
    Latest = 1,
    Supported = 2,
    Deprecated = 3,
    Unsupported = 4
}

/// <summary>
/// DTO لعرض بيانات الإصدار
/// </summary>
public sealed class AppVersionDto
{
    public Guid Id { get; set; }
    public string ApplicationCode { get; set; } = string.Empty;
    public string ApplicationNameAr { get; set; } = string.Empty;
    public string ApplicationNameEn { get; set; } = string.Empty;
    public string VersionNumber { get; set; } = string.Empty;
    public int BuildNumber { get; set; }
    public VersionStatus Status { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public DateTime ReleaseDate { get; set; }
    public DateTime? DeprecationStartDate { get; set; }
    public DateTime? EndOfSupportDate { get; set; }
    public string? ReleaseNotesAr { get; set; }
    public string? ReleaseNotesEn { get; set; }
    public string? UpdateUrl { get; set; }
    public string? DownloadUrl { get; set; }
    public bool IsForceUpdate { get; set; }
    public string? MinimumSupportedVersion { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// طلب إنشاء إصدار جديد
/// </summary>
public sealed class CreateAppVersionRequest
{
    public string ApplicationCode { get; set; } = string.Empty;
    public string ApplicationNameAr { get; set; } = string.Empty;
    public string ApplicationNameEn { get; set; } = string.Empty;
    public string VersionNumber { get; set; } = string.Empty;
    public int BuildNumber { get; set; }
    public VersionStatus Status { get; set; } = VersionStatus.Development;
    public DateTime ReleaseDate { get; set; } = DateTime.UtcNow;
    public DateTime? DeprecationStartDate { get; set; }
    public DateTime? EndOfSupportDate { get; set; }
    public string? ReleaseNotesAr { get; set; }
    public string? ReleaseNotesEn { get; set; }
    public string? UpdateUrl { get; set; }
    public string? DownloadUrl { get; set; }
    public bool IsForceUpdate { get; set; }
    public string? MinimumSupportedVersion { get; set; }
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// طلب تحديث إصدار
/// </summary>
public sealed class UpdateAppVersionRequest
{
    public string? ApplicationNameAr { get; set; }
    public string? ApplicationNameEn { get; set; }
    public VersionStatus? Status { get; set; }
    public DateTime? DeprecationStartDate { get; set; }
    public DateTime? EndOfSupportDate { get; set; }
    public string? ReleaseNotesAr { get; set; }
    public string? ReleaseNotesEn { get; set; }
    public string? UpdateUrl { get; set; }
    public string? DownloadUrl { get; set; }
    public bool? IsForceUpdate { get; set; }
    public string? MinimumSupportedVersion { get; set; }
    public bool? IsActive { get; set; }
}

/// <summary>
/// طلب فحص الإصدار
/// </summary>
public sealed class VersionCheckRequest
{
    public string ApplicationCode { get; set; } = string.Empty;
    public string CurrentVersion { get; set; } = string.Empty;
    public int? CurrentBuildNumber { get; set; }
}

/// <summary>
/// نتيجة فحص الإصدار
/// </summary>
public sealed class VersionCheckResult
{
    /// <summary>
    /// هل الإصدار الحالي مدعوم؟
    /// </summary>
    public bool IsSupported { get; set; }

    /// <summary>
    /// هل يوجد تحديث متاح؟
    /// </summary>
    public bool UpdateAvailable { get; set; }

    /// <summary>
    /// هل التحديث إجباري؟
    /// </summary>
    public bool IsForceUpdate { get; set; }

    /// <summary>
    /// حالة الإصدار الحالي
    /// </summary>
    public VersionStatus CurrentStatus { get; set; }

    /// <summary>
    /// رسالة للمستخدم بالعربية
    /// </summary>
    public string? MessageAr { get; set; }

    /// <summary>
    /// رسالة للمستخدم بالإنجليزية
    /// </summary>
    public string? MessageEn { get; set; }

    /// <summary>
    /// تاريخ انتهاء الدعم
    /// </summary>
    public DateTime? EndOfSupportDate { get; set; }

    /// <summary>
    /// أحدث إصدار متاح
    /// </summary>
    public AppVersionDto? LatestVersion { get; set; }

    /// <summary>
    /// بيانات الإصدار الحالي
    /// </summary>
    public AppVersionDto? CurrentVersionInfo { get; set; }
}

#endregion
