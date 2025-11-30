namespace ACommerce.Spaces.Enums;

/// <summary>
/// نوع التسعير
/// </summary>
public enum PricingType
{
    /// <summary>
    /// بالساعة
    /// </summary>
    Hourly = 1,

    /// <summary>
    /// بالنصف يوم (4 ساعات)
    /// </summary>
    HalfDay = 2,

    /// <summary>
    /// باليوم
    /// </summary>
    Daily = 3,

    /// <summary>
    /// بالأسبوع
    /// </summary>
    Weekly = 4,

    /// <summary>
    /// بالشهر
    /// </summary>
    Monthly = 5,

    /// <summary>
    /// بالسنة
    /// </summary>
    Yearly = 6,

    /// <summary>
    /// بالشخص (للمساحات المشتركة)
    /// </summary>
    PerPerson = 7,

    /// <summary>
    /// سعر ثابت للحدث
    /// </summary>
    FlatRate = 8
}
