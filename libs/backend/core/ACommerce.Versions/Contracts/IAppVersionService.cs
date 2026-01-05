using ACommerce.Versions.DTOs;
using ACommerce.Versions.Enums;

namespace ACommerce.Versions.Contracts;

/// <summary>
/// واجهة خدمة إدارة الإصدارات
/// </summary>
public interface IAppVersionService
{
    /// <summary>
    /// الحصول على جميع الإصدارات
    /// </summary>
    Task<IEnumerable<AppVersionDto>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// الحصول على الإصدارات النشطة فقط
    /// </summary>
    Task<IEnumerable<AppVersionDto>> GetActiveAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// الحصول على إصدار بواسطة المعرف
    /// </summary>
    Task<AppVersionDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// الحصول على إصدارات تطبيق معين
    /// </summary>
    Task<IEnumerable<AppVersionDto>> GetByApplicationCodeAsync(string applicationCode, CancellationToken cancellationToken = default);

    /// <summary>
    /// الحصول على إصدار معين لتطبيق
    /// </summary>
    Task<AppVersionDto?> GetByVersionAsync(string applicationCode, string versionNumber, CancellationToken cancellationToken = default);

    /// <summary>
    /// الحصول على أحدث إصدار لتطبيق
    /// </summary>
    Task<AppVersionDto?> GetLatestVersionAsync(string applicationCode, CancellationToken cancellationToken = default);

    /// <summary>
    /// الحصول على الإصدارات حسب الحالة
    /// </summary>
    Task<IEnumerable<AppVersionDto>> GetByStatusAsync(VersionStatus status, CancellationToken cancellationToken = default);

    /// <summary>
    /// فحص الإصدار للعميل
    /// </summary>
    Task<VersionCheckResult> CheckVersionAsync(VersionCheckRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// إنشاء إصدار جديد
    /// </summary>
    Task<AppVersionDto> CreateAsync(CreateAppVersionRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// تحديث إصدار
    /// </summary>
    Task<AppVersionDto?> UpdateAsync(Guid id, UpdateAppVersionRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// تغيير حالة الإصدار
    /// </summary>
    Task<AppVersionDto?> ChangeStatusAsync(Guid id, VersionStatus newStatus, CancellationToken cancellationToken = default);

    /// <summary>
    /// تفعيل/تعطيل إصدار
    /// </summary>
    Task<AppVersionDto?> ToggleActiveAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// حذف إصدار (soft delete)
    /// </summary>
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// الحصول على قائمة التطبيقات المسجلة
    /// </summary>
    Task<IEnumerable<string>> GetApplicationCodesAsync(CancellationToken cancellationToken = default);
}
