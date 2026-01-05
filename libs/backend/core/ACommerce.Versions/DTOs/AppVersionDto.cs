using ACommerce.Versions.Enums;

namespace ACommerce.Versions.DTOs;

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
    public string? Metadata { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// DTO لإنشاء إصدار جديد
/// </summary>
public sealed class CreateAppVersionRequest
{
    public required string ApplicationCode { get; set; }
    public required string ApplicationNameAr { get; set; }
    public required string ApplicationNameEn { get; set; }
    public required string VersionNumber { get; set; }
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
    public string? Metadata { get; set; }
}

/// <summary>
/// DTO لتحديث إصدار
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
    public string? Metadata { get; set; }
}

/// <summary>
/// DTO للتحقق من الإصدار من قبل العميل
/// </summary>
public sealed class VersionCheckRequest
{
    public required string ApplicationCode { get; set; }
    public required string CurrentVersion { get; set; }
    public int? CurrentBuildNumber { get; set; }
}

/// <summary>
/// DTO لنتيجة فحص الإصدار
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
    /// تاريخ انتهاء الدعم (إذا كان الإصدار deprecated)
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
