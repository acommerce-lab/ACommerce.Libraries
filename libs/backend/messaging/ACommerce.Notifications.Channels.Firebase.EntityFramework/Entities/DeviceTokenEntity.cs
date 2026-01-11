using ACommerce.SharedKernel.Abstractions.Entities;

namespace ACommerce.Notifications.Channels.Firebase.EntityFramework.Entities;

/// <summary>
/// كيان تخزين Device Token في قاعدة البيانات
/// </summary>
public class DeviceTokenEntity : IBaseEntity
{
    /// <summary>
    /// المعرف الفريد
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// معرف المستخدم
    /// </summary>
    public string UserId { get; set; } = default!;

    /// <summary>
    /// Firebase Cloud Messaging Token
    /// </summary>
    public string Token { get; set; } = default!;

    /// <summary>
    /// نوع المنصة (iOS, Android, Web)
    /// </summary>
    public string Platform { get; set; } = default!;

    /// <summary>
    /// تاريخ التسجيل
    /// </summary>
    public DateTime RegisteredAt { get; set; }

    /// <summary>
    /// تاريخ آخر استخدام
    /// </summary>
    public DateTime LastUsedAt { get; set; }

    /// <summary>
    /// هل التوكن نشط
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// إصدار التطبيق
    /// </summary>
    public string? AppVersion { get; set; }

    /// <summary>
    /// موديل الجهاز
    /// </summary>
    public string? DeviceModel { get; set; }

    /// <summary>
    /// بيانات إضافية (JSON)
    /// </summary>
    public string? MetadataJson { get; set; }

    // IBaseEntity implementation
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }
}
