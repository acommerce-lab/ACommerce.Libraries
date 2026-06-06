using ACommerce.SharedKernel.Abstractions.Entities;

namespace ACommerce.AppConfig.Entities;

/// <summary>
/// علامة ميزة — تتحكم في تفعيل ميزات التطبيق بدون نشر تحديث.
/// مستويات التحكم: Enabled، Platforms، MinVersion، MaxVersion.
/// مفتاح فريد على Key.
/// </summary>
public class FeatureFlag : IBaseEntity
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }

    /// <summary>المفتاح بالتدوين النقطي (مثلاً payments.noon، booking.enabled).</summary>
    public required string Key { get; set; }

    /// <summary>الحالة العامة — التعطيل يلغي الميزة لكل المستخدمين.</summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// قائمة المنصات المسموحة (CSV): android, ios, web، أو null = كل المنصات.
    /// </summary>
    public string? Platforms { get; set; }

    /// <summary>
    /// أدنى إصدار تطبيق يرى هذه الميزة (Semantic version: "1.16").
    /// null = بدون حد أدنى. عميل بإصدار أقل لا يحصل على الميزة.
    /// </summary>
    public string? MinAppVersion { get; set; }

    /// <summary>
    /// أعلى إصدار تطبيق يرى هذه الميزة (Semantic version).
    /// null = بدون حد أعلى. مفيد لإيقاف ميزة في إصدارات قديمة بها bug.
    /// </summary>
    public string? MaxAppVersion { get; set; }

    /// <summary>وصف الميزة (يظهر في لوحة الإدارة).</summary>
    public string? Description { get; set; }

    /// <summary>
    /// تقدير: هل هذه الميزة تتطلب تجاوز إعادة بناء الـ App؟ (إعلامية فقط).
    /// </summary>
    public bool RequiresClientRestart { get; set; }
}
