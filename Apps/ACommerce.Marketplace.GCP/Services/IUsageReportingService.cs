namespace ACommerce.Marketplace.GCP.Services;

/// <summary>
/// Defines usage metrics that can be reported to GCP Marketplace for billing.
/// These correspond to the pricing model defined in your Marketplace listing.
/// </summary>
public enum UsageMetricType
{
    /// <summary>
    /// Number of API requests made
    /// </summary>
    ApiRequests,

    /// <summary>
    /// Number of orders created
    /// </summary>
    OrdersCreated,

    /// <summary>
    /// Number of products in catalog
    /// </summary>
    ProductsCatalogued,

    /// <summary>
    /// Number of active vendors/sellers
    /// </summary>
    ActiveVendors,

    /// <summary>
    /// Storage used in MB
    /// </summary>
    StorageUsedMB,

    /// <summary>
    /// Bandwidth used in GB
    /// </summary>
    BandwidthUsedGB,

    /// <summary>
    /// Number of active users
    /// </summary>
    ActiveUsers,

    /// <summary>
    /// Total transaction value (for percentage-based billing)
    /// </summary>
    TransactionValue
}

/// <summary>
/// Service interface for reporting usage to GCP Marketplace.
/// This is the central hook point for all billing-related usage tracking.
///
/// Usage:
/// 1. Inject IUsageReportingService where usage needs to be tracked
/// 2. Call RecordUsage() when billable events occur
/// 3. The service handles batching, retries, and reporting to GCP
///
/// Example integration points:
/// - Order creation: Record OrdersCreated metric
/// - API middleware: Record ApiRequests metric
/// - File upload: Record StorageUsedMB metric
/// </summary>
public interface IUsageReportingService
{
    /// <summary>
    /// Records a usage metric for the current customer.
    /// Usage is batched and reported to GCP Marketplace periodically.
    /// </summary>
    /// <param name="metricType">The type of metric to record</param>
    /// <param name="value">The metric value (e.g., 1 for a single event, bytes for storage)</param>
    /// <param name="metadata">Optional metadata for the usage record</param>
    Task RecordUsageAsync(UsageMetricType metricType, long value, Dictionary<string, string>? metadata = null);

    /// <summary>
    /// Records usage with a custom metric name.
    /// Use this for metrics not covered by UsageMetricType.
    /// </summary>
    /// <param name="metricName">Custom metric name as defined in your Marketplace listing</param>
    /// <param name="value">The metric value</param>
    /// <param name="metadata">Optional metadata</param>
    Task RecordUsageAsync(string metricName, long value, Dictionary<string, string>? metadata = null);

    /// <summary>
    /// Forces immediate flush of all pending usage records to GCP.
    /// Called automatically on shutdown, but can be invoked manually.
    /// </summary>
    Task FlushAsync();

    /// <summary>
    /// Gets the current reporting status
    /// </summary>
    UsageReportingStatus GetStatus();
}

/// <summary>
/// Status of the usage reporting service
/// </summary>
public class UsageReportingStatus
{
    public bool IsEnabled { get; set; }
    public bool IsConfigured { get; set; }
    public long PendingRecords { get; set; }
    public DateTime? LastReportTime { get; set; }
    public string? LastError { get; set; }
}
