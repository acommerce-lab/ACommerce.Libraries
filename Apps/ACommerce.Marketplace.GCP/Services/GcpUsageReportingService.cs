using System.Collections.Concurrent;
using Microsoft.Extensions.Options;
using ACommerce.Marketplace.GCP.Configuration;

namespace ACommerce.Marketplace.GCP.Services;

/// <summary>
/// Implementation of usage reporting for Google Cloud Marketplace.
///
/// This service:
/// 1. Collects usage metrics from various parts of the application
/// 2. Batches them for efficient reporting
/// 3. Sends them to GCP Service Control API for billing
///
/// INTEGRATION POINTS FOR BILLING:
/// ================================
///
/// 1. ORDER CREATION (libs/backend/sales/ACommerce.Orders):
///    When an order is confirmed, inject this service and call:
///    await _usageReporting.RecordUsageAsync(UsageMetricType.OrdersCreated, 1);
///    await _usageReporting.RecordUsageAsync(UsageMetricType.TransactionValue, (long)(order.Total * 100));
///
/// 2. API REQUESTS (Middleware):
///    Add middleware to track API calls:
///    await _usageReporting.RecordUsageAsync(UsageMetricType.ApiRequests, 1);
///
/// 3. VENDOR REGISTRATION:
///    When a vendor is activated:
///    await _usageReporting.RecordUsageAsync(UsageMetricType.ActiveVendors, 1);
///
/// 4. FILE STORAGE:
///    When files are uploaded:
///    await _usageReporting.RecordUsageAsync(UsageMetricType.StorageUsedMB, fileSizeMB);
/// </summary>
public class GcpUsageReportingService : IUsageReportingService, IDisposable
{
    private readonly GcpBillingOptions _options;
    private readonly ILogger<GcpUsageReportingService> _logger;
    private readonly ConcurrentQueue<UsageRecord> _pendingRecords = new();
    private readonly Timer _flushTimer;
    private DateTime? _lastReportTime;
    private string? _lastError;
    private bool _disposed;

    public GcpUsageReportingService(
        IOptions<GcpBillingOptions> options,
        ILogger<GcpUsageReportingService> logger)
    {
        _options = options.Value;
        _logger = logger;

        // Setup periodic flush timer
        var interval = TimeSpan.FromSeconds(_options.UsageReportIntervalSeconds);
        _flushTimer = new Timer(
            callback: async _ => await FlushAsync(),
            state: null,
            dueTime: interval,
            period: interval);

        if (_options.EnableUsageReporting)
        {
            if (_options.IsValid())
            {
                _logger.LogInformation(
                    "GCP Usage Reporting enabled for service {ServiceName}, consumer {ConsumerId}",
                    _options.ServiceName,
                    _options.ConsumerId);
            }
            else
            {
                _logger.LogWarning(
                    "GCP Usage Reporting is enabled but not properly configured. " +
                    "Check GCP_PROJECT_ID, GCP_SERVICE_NAME, GCP_ENTITLEMENT_ID, GCP_CONSUMER_ID");
            }
        }
        else
        {
            _logger.LogInformation("GCP Usage Reporting is disabled");
        }
    }

    public Task RecordUsageAsync(UsageMetricType metricType, long value, Dictionary<string, string>? metadata = null)
    {
        var metricName = metricType switch
        {
            UsageMetricType.ApiRequests => "api_requests",
            UsageMetricType.OrdersCreated => "orders_created",
            UsageMetricType.ProductsCatalogued => "products_catalogued",
            UsageMetricType.ActiveVendors => "active_vendors",
            UsageMetricType.StorageUsedMB => "storage_used_mb",
            UsageMetricType.BandwidthUsedGB => "bandwidth_used_gb",
            UsageMetricType.ActiveUsers => "active_users",
            UsageMetricType.TransactionValue => "transaction_value_cents",
            _ => metricType.ToString().ToLowerInvariant()
        };

        return RecordUsageAsync(metricName, value, metadata);
    }

    public Task RecordUsageAsync(string metricName, long value, Dictionary<string, string>? metadata = null)
    {
        if (!_options.EnableUsageReporting)
            return Task.CompletedTask;

        var record = new UsageRecord
        {
            MetricName = metricName,
            Value = value,
            Timestamp = DateTime.UtcNow,
            Metadata = metadata ?? new Dictionary<string, string>()
        };

        _pendingRecords.Enqueue(record);

        _logger.LogDebug(
            "Recorded usage: {MetricName}={Value} (pending: {PendingCount})",
            metricName,
            value,
            _pendingRecords.Count);

        return Task.CompletedTask;
    }

    public async Task FlushAsync()
    {
        if (!_options.EnableUsageReporting || !_options.IsValid())
            return;

        var recordsToSend = new List<UsageRecord>();

        // Drain the queue
        while (_pendingRecords.TryDequeue(out var record))
        {
            recordsToSend.Add(record);
        }

        if (recordsToSend.Count == 0)
            return;

        try
        {
            _logger.LogInformation(
                "Flushing {Count} usage records to GCP Service Control",
                recordsToSend.Count);

            // Aggregate records by metric name
            var aggregated = recordsToSend
                .GroupBy(r => r.MetricName)
                .Select(g => new
                {
                    MetricName = g.Key,
                    TotalValue = g.Sum(r => r.Value),
                    Count = g.Count()
                })
                .ToList();

            // TODO: Implement actual GCP Service Control API call
            // This is where you would call the Google Cloud Service Control API
            // to report usage for billing.
            //
            // Example using Google.Cloud.ServiceControl.V1:
            // var client = ServiceControllerClient.Create();
            // var request = new ReportRequest
            // {
            //     ServiceName = _options.ServiceName,
            //     Operations = { ... }
            // };
            // await client.ReportAsync(request);

            // For now, log what would be sent
            foreach (var metric in aggregated)
            {
                _logger.LogInformation(
                    "Would report to GCP: {ServiceName}/{MetricName} = {Value} (from {Count} records)",
                    _options.ServiceName,
                    metric.MetricName,
                    metric.TotalValue,
                    metric.Count);
            }

            _lastReportTime = DateTime.UtcNow;
            _lastError = null;
        }
        catch (Exception ex)
        {
            _lastError = ex.Message;
            _logger.LogError(ex, "Failed to flush usage records to GCP");

            // Re-queue records for retry
            foreach (var record in recordsToSend)
            {
                _pendingRecords.Enqueue(record);
            }
        }
    }

    public UsageReportingStatus GetStatus()
    {
        return new UsageReportingStatus
        {
            IsEnabled = _options.EnableUsageReporting,
            IsConfigured = _options.IsValid(),
            PendingRecords = _pendingRecords.Count,
            LastReportTime = _lastReportTime,
            LastError = _lastError
        };
    }

    public void Dispose()
    {
        if (_disposed) return;

        _disposed = true;
        _flushTimer.Dispose();

        // Final flush on shutdown
        FlushAsync().GetAwaiter().GetResult();
    }

    private class UsageRecord
    {
        public required string MetricName { get; set; }
        public long Value { get; set; }
        public DateTime Timestamp { get; set; }
        public Dictionary<string, string> Metadata { get; set; } = new();
    }
}
