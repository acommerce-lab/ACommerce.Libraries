namespace ACommerce.Marketplace.GCP.Configuration;

/// <summary>
/// Configuration options for Google Cloud Marketplace billing integration.
/// All values are injected via environment variables by GCP Marketplace.
/// </summary>
public class GcpBillingOptions
{
    /// <summary>
    /// Google Cloud Project ID where the service is deployed
    /// Environment: GCP_PROJECT_ID
    /// </summary>
    public string ProjectId { get; set; } = string.Empty;

    /// <summary>
    /// Service name as registered in GCP Marketplace
    /// Environment: GCP_SERVICE_NAME
    /// </summary>
    public string ServiceName { get; set; } = "acommerce-marketplace";

    /// <summary>
    /// The entitlement ID for the customer's subscription
    /// Injected by GCP Marketplace at container startup
    /// Environment: GCP_ENTITLEMENT_ID
    /// </summary>
    public string EntitlementId { get; set; } = string.Empty;

    /// <summary>
    /// The consumer ID (customer project or account)
    /// Injected by GCP Marketplace at container startup
    /// Environment: GCP_CONSUMER_ID
    /// </summary>
    public string ConsumerId { get; set; } = string.Empty;

    /// <summary>
    /// Enable/disable usage reporting
    /// Set to true in production for billing integration
    /// Environment: GCP_ENABLE_USAGE_REPORTING
    /// </summary>
    public bool EnableUsageReporting { get; set; } = false;

    /// <summary>
    /// Service Control API endpoint (usually auto-configured)
    /// Environment: GCP_SERVICE_CONTROL_ENDPOINT
    /// </summary>
    public string ServiceControlEndpoint { get; set; } = "https://servicecontrol.googleapis.com";

    /// <summary>
    /// Interval in seconds between usage report batches
    /// Default: 60 seconds
    /// Environment: GCP_USAGE_REPORT_INTERVAL_SECONDS
    /// </summary>
    public int UsageReportIntervalSeconds { get; set; } = 60;

    /// <summary>
    /// Validates that required configuration is present for usage reporting
    /// </summary>
    public bool IsValid()
    {
        if (!EnableUsageReporting) return true;

        return !string.IsNullOrEmpty(ProjectId) &&
               !string.IsNullOrEmpty(ServiceName) &&
               !string.IsNullOrEmpty(EntitlementId) &&
               !string.IsNullOrEmpty(ConsumerId);
    }
}
