namespace ACommerce.AccountingKernel.Abstractions;

/// <summary>
/// خيارات تشغيل المحرك - يضبطها مصمم التطبيق عبر DI.
/// المحرك يسأل هذه الخيارات: هل أسجل؟ هل أوثّق؟ هل أحفظ كيانات؟
/// </summary>
public class EntryEngineOptions
{
    /// <summary>
    /// هل يوثّق المحرك القيود في سجل العمليات (IEntryStore)؟
    /// </summary>
    public bool EnableAudit { get; set; }

    /// <summary>
    /// هل يحفظ المحرك الكيانات عبر IPersistenceGateway؟
    /// </summary>
    public bool EnableEntityPersistence { get; set; }

    /// <summary>
    /// هل ينشر المحرك الأحداث عبر IEventPublisher؟
    /// </summary>
    public bool EnableEvents { get; set; } = true;

    /// <summary>
    /// هل يفحص المحرك التوازن (مجموع المدين = مجموع الدائن)؟
    /// يمكن تعطيله للعمليات أحادية الطرف.
    /// </summary>
    public bool EnforceBalance { get; set; } = true;
}
