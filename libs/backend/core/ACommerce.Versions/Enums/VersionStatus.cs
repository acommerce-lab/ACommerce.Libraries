namespace ACommerce.Versions.Enums;

/// <summary>
/// حالة دعم الإصدار
/// </summary>
public enum VersionStatus
{
    /// <summary>
    /// قيد التطوير - غير متاح للاستخدام العام
    /// </summary>
    Development = 0,

    /// <summary>
    /// أحدث إصدار - الإصدار الموصى به
    /// </summary>
    Latest = 1,

    /// <summary>
    /// مدعوم - إصدار قديم لكن لا يزال مدعوماً
    /// </summary>
    Supported = 2,

    /// <summary>
    /// على وشك رفع الدعم - سيتم إيقاف الدعم قريباً
    /// </summary>
    Deprecated = 3,

    /// <summary>
    /// غير مدعوم - تم إيقاف الدعم نهائياً
    /// </summary>
    Unsupported = 4
}
