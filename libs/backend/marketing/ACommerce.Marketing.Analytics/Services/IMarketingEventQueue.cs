using ACommerce.Marketing.Analytics.Entities;

namespace ACommerce.Marketing.Analytics.Services;

/// <summary>
/// طابور لمعالجة الأحداث التسويقية في الخلفية
/// يسمح بإرسال الأحداث بدون حجب العملية الرئيسية
/// </summary>
public interface IMarketingEventQueue
{
    /// <summary>
    /// إضافة حدث للطابور للمعالجة في الخلفية
    /// </summary>
    void Enqueue(MarketingQueueItem item);

    /// <summary>
    /// قراءة الحدث التالي من الطابور
    /// </summary>
    ValueTask<MarketingQueueItem> DequeueAsync(CancellationToken cancellationToken);
}

/// <summary>
/// عنصر في طابور الأحداث التسويقية
/// </summary>
public class MarketingQueueItem
{
    public required MarketingEventType EventType { get; init; }
    public required object Request { get; init; }
    public DateTime EnqueuedAt { get; init; } = DateTime.UtcNow;
}
