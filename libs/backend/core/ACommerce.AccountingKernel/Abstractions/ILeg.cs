namespace ACommerce.AccountingKernel.Abstractions;

/// <summary>
/// طرف في قيد محاسبي عملياتي.
/// كل قيد يتكون من أطراف مدينة ودائنة يجب أن تتوازن.
/// الطرف لا يمثل المال فقط - يمثل أي شيء يُنقل بين جهتين.
/// </summary>
public interface ILeg
{
    /// <summary>
    /// معرف الطرف
    /// </summary>
    Guid Id { get; }

    /// <summary>
    /// اسم الجهة المالكة لهذا الطرف (مثل: "System", "User:123", "Channel:Email")
    /// </summary>
    string Party { get; }

    /// <summary>
    /// اتجاه الطرف: مدين (يُعطي) أو دائن (يستلم)
    /// </summary>
    LegDirection Direction { get; }

    /// <summary>
    /// نوع ما يُنقل (مثل: "Money", "Notification", "Message", "Token", "Inventory")
    /// </summary>
    string ResourceType { get; }

    /// <summary>
    /// قيمة رقمية (اختيارية) - تُستخدم للتوازن الكمي
    /// </summary>
    decimal Value { get; }

    /// <summary>
    /// الحالة الحالية للطرف
    /// </summary>
    LegStatus Status { get; set; }

    /// <summary>
    /// بيانات إضافية مرتبطة بهذا الطرف
    /// يمكن أن تحمل كيان IBaseEntity أو أي بيانات مخصصة
    /// </summary>
    Dictionary<string, object> Payload { get; }
}

/// <summary>
/// اتجاه الطرف في القيد
/// </summary>
public enum LegDirection
{
    /// <summary>
    /// مدين: الجهة التي تُعطي/تُرسل/تُصدر
    /// </summary>
    Debit,

    /// <summary>
    /// دائن: الجهة التي تستلم/تحصل
    /// </summary>
    Credit
}

/// <summary>
/// حالة الطرف في دورة حياته
/// </summary>
public enum LegStatus
{
    /// <summary>
    /// منتظر التنفيذ
    /// </summary>
    Pending,

    /// <summary>
    /// قيد التنفيذ
    /// </summary>
    InProgress,

    /// <summary>
    /// اكتمل بنجاح
    /// </summary>
    Completed,

    /// <summary>
    /// فشل
    /// </summary>
    Failed,

    /// <summary>
    /// تم الإلغاء
    /// </summary>
    Cancelled
}
