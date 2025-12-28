namespace ACommerce.Marketing.Analytics.Services;

/// <summary>
/// معالج الأحداث التسويقية الفعلي
/// يُستخدم من قبل BackgroundService لمعالجة الأحداث من الطابور
/// </summary>
public interface IMarketingEventProcessor
{
    /// <summary>
    /// معالجة حدث من الطابور
    /// </summary>
    Task ProcessEventAsync(MarketingQueueItem item, CancellationToken cancellationToken = default);
}
