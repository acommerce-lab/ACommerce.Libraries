using ACommerce.SharedKernel.Abstractions.Entities;
using ACommerce.Versions.Enums;

namespace ACommerce.Versions.Entities;

/// <summary>
/// كيان إصدار التطبيق
/// </summary>
public class AppVersion : IBaseEntity
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }

    /// <summary>
    /// رمز التطبيق (مثال: customer-app, admin-panel, mobile-ios)
    /// </summary>
    public string ApplicationCode { get; set; } = string.Empty;

    /// <summary>
    /// اسم التطبيق بالعربية
    /// </summary>
    public string ApplicationNameAr { get; set; } = string.Empty;

    /// <summary>
    /// اسم التطبيق بالإنجليزية
    /// </summary>
    public string ApplicationNameEn { get; set; } = string.Empty;

    /// <summary>
    /// رقم الإصدار (مثال: 1.0.0, 2.1.3)
    /// </summary>
    public string VersionNumber { get; set; } = string.Empty;

    /// <summary>
    /// رمز البناء (Build Number)
    /// </summary>
    public int BuildNumber { get; set; }

    /// <summary>
    /// حالة دعم الإصدار
    /// </summary>
    public VersionStatus Status { get; set; } = VersionStatus.Development;

    /// <summary>
    /// تاريخ إصدار النسخة
    /// </summary>
    public DateTime ReleaseDate { get; set; }

    /// <summary>
    /// تاريخ بداية فترة الإيقاف (deprecated) - متى سيبدأ التحذير
    /// </summary>
    public DateTime? DeprecationStartDate { get; set; }

    /// <summary>
    /// تاريخ نهاية الدعم - متى سيصبح الإصدار غير مدعوم
    /// </summary>
    public DateTime? EndOfSupportDate { get; set; }

    /// <summary>
    /// ملاحظات الإصدار بالعربية
    /// </summary>
    public string? ReleaseNotesAr { get; set; }

    /// <summary>
    /// ملاحظات الإصدار بالإنجليزية
    /// </summary>
    public string? ReleaseNotesEn { get; set; }

    /// <summary>
    /// رابط التحديث (للتطبيقات)
    /// </summary>
    public string? UpdateUrl { get; set; }

    /// <summary>
    /// رابط التحميل
    /// </summary>
    public string? DownloadUrl { get; set; }

    /// <summary>
    /// هل التحديث إجباري؟
    /// </summary>
    public bool IsForceUpdate { get; set; }

    /// <summary>
    /// الحد الأدنى للإصدار المدعوم (للتطبيقات التي تحتاج توافق)
    /// </summary>
    public string? MinimumSupportedVersion { get; set; }

    /// <summary>
    /// هل الإصدار نشط؟
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// بيانات إضافية (JSON)
    /// </summary>
    public string? Metadata { get; set; }
}
