namespace ACommerce.Templates.Customer.Components;

/// <summary>
/// نوع تنبيه الإصدار
/// </summary>
public enum VersionAlertType
{
    /// <summary>
    /// معلومات - تحديث متاح (أزرق)
    /// </summary>
    Info = 0,

    /// <summary>
    /// تحذير - إصدار على وشك الانتهاء (برتقالي)
    /// </summary>
    Warning = 1,

    /// <summary>
    /// حرج - يجب التحديث فوراً (أحمر)
    /// </summary>
    Critical = 2
}
