using ACommerce.Marketing.Analytics.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ACommerce.Marketing.Analytics.Services;

/// <summary>
/// خدمة خلفية لمعالجة الأحداث التسويقية من الطابور
/// تعمل بشكل مستمر وتعالج الأحداث بدون حجب العمليات الرئيسية
/// </summary>
public class MarketingEventBackgroundService : BackgroundService
{
    private readonly IMarketingEventQueue _queue;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<MarketingEventBackgroundService> _logger;

    public MarketingEventBackgroundService(
        IMarketingEventQueue queue,
        IServiceScopeFactory scopeFactory,
        ILogger<MarketingEventBackgroundService> logger)
    {
        _queue = queue;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("[MarketingBackgroundService] Started processing marketing events queue");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var item = await _queue.DequeueAsync(stoppingToken);
                await ProcessItemAsync(item, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                // التطبيق يتوقف - هذا طبيعي
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[MarketingBackgroundService] Error processing queue item");
                // ننتظر قليلاً قبل المحاولة مرة أخرى
                await Task.Delay(1000, stoppingToken);
            }
        }

        _logger.LogInformation("[MarketingBackgroundService] Stopped processing marketing events queue");
    }

    private async Task ProcessItemAsync(MarketingQueueItem item, CancellationToken cancellationToken)
    {
        var processingTime = DateTime.UtcNow - item.EnqueuedAt;
        _logger.LogDebug("[MarketingBackgroundService] Processing {EventType} event (queued {Ms}ms ago)",
            item.EventType, processingTime.TotalMilliseconds);

        // نحتاج scope جديد لأن الخدمات scoped
        using var scope = _scopeFactory.CreateScope();
        var tracker = scope.ServiceProvider.GetRequiredService<IMarketingEventProcessor>();

        try
        {
            await tracker.ProcessEventAsync(item, cancellationToken);
            _logger.LogInformation("✅ [MarketingBackgroundService] {EventType} event processed successfully",
                item.EventType);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "⚠️ [MarketingBackgroundService] Failed to process {EventType} event",
                item.EventType);
        }
    }
}
