namespace ACommerce.AccountingKernel.Abstractions;

/// <summary>
/// قيد عملياتي: الوحدة الذرية لكل عملية في النظام.
/// كل تفاعل بين طرفين هو قيد له أطراف وشروط ودورة حياة.
/// </summary>
public interface IEntry
{
    /// <summary>
    /// معرف القيد الفريد
    /// </summary>
    Guid Id { get; }

    /// <summary>
    /// نوع القيد (مثل: "notification.send", "chat.message", "auth.login", "order.place")
    /// </summary>
    string EntryType { get; }

    /// <summary>
    /// وصف مقروء للقيد
    /// </summary>
    string? Description { get; }

    /// <summary>
    /// أطراف القيد (مدين ودائن)
    /// </summary>
    IReadOnlyList<ILeg> Legs { get; }

    /// <summary>
    /// قيود فرعية (مثل: إشعار رئيسي → قيد إرسال لكل قناة)
    /// </summary>
    IReadOnlyList<IEntry> SubEntries { get; }

    /// <summary>
    /// القيد الأب (إن كان هذا قيداً فرعياً)
    /// </summary>
    Guid? ParentEntryId { get; }

    /// <summary>
    /// حالة القيد
    /// </summary>
    EntryStatus Status { get; }

    /// <summary>
    /// وقت الإنشاء
    /// </summary>
    DateTime CreatedAt { get; }

    /// <summary>
    /// وقت الاكتمال
    /// </summary>
    DateTime? CompletedAt { get; }

    /// <summary>
    /// بيانات وصفية
    /// </summary>
    Dictionary<string, object> Metadata { get; }
}

/// <summary>
/// حالات دورة حياة القيد
/// </summary>
public enum EntryStatus
{
    /// <summary>
    /// أُنشئ ولم يُنفذ بعد
    /// </summary>
    Created,

    /// <summary>
    /// التحقق المسبق نجح، جاهز للتنفيذ
    /// </summary>
    Validated,

    /// <summary>
    /// قيد التنفيذ
    /// </summary>
    Executing,

    /// <summary>
    /// اكتمل بنجاح (كل الأطراف والقيود الفرعية اكتملت)
    /// </summary>
    Completed,

    /// <summary>
    /// اكتمل جزئياً (بعض الأطراف نجحت وبعضها معلّق)
    /// </summary>
    PartiallyCompleted,

    /// <summary>
    /// فشل
    /// </summary>
    Failed,

    /// <summary>
    /// تم عكسه (Reversed)
    /// </summary>
    Reversed,

    /// <summary>
    /// تم إلغاؤه
    /// </summary>
    Cancelled
}
