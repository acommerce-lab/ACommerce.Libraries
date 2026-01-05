using System.Threading.Channels;
using ACommerce.Marketing.Analytics.Entities;
using Microsoft.Extensions.Logging;

namespace ACommerce.Marketing.Analytics.Services;

/// <summary>
/// تنفيذ طابور الأحداث التسويقية باستخدام Channel
/// يتيح معالجة الأحداث في الخلفية بدون حجب العمليات الرئيسية
/// </summary>
public class MarketingEventQueue : IMarketingEventQueue
{
    private readonly Channel<MarketingQueueItem> _channel;
    private readonly ILogger<MarketingEventQueue> _logger;

    public MarketingEventQueue(ILogger<MarketingEventQueue> logger)
    {
        _logger = logger;

        // طابور غير محدود (يمكن تحديده لاحقاً إذا لزم الأمر)
        _channel = Channel.CreateUnbounded<MarketingQueueItem>(new UnboundedChannelOptions
        {
            SingleReader = true,  // قارئ واحد فقط (BackgroundService)
            SingleWriter = false  // كتّاب متعددون (Controllers)
        });
    }

    public void Enqueue(MarketingQueueItem item)
    {
        if (!_channel.Writer.TryWrite(item))
        {
            _logger.LogError("[MarketingQueue] ❌ Failed to enqueue {EventType} event - queue full or closed", item.EventType);
            return;
        }

        _logger.LogInformation("[MarketingQueue] 📥 Enqueued {EventType} event", item.EventType);
    }

    public async ValueTask<MarketingQueueItem> DequeueAsync(CancellationToken cancellationToken)
    {
        return await _channel.Reader.ReadAsync(cancellationToken);
    }
}
