namespace ACommerce.Marketing.GoogleConversions;

/// <summary>
/// Configuration options for Google Analytics 4 Measurement Protocol
/// </summary>
public class GoogleConversionsOptions
{
    public const string SectionName = "GoogleConversions";

    /// <summary>
    /// GA4 Measurement ID (e.g., G-XXXXXXX)
    /// </summary>
    public string MeasurementId { get; set; } = string.Empty;

    /// <summary>
    /// GA4 API Secret from Admin > Data Streams > Measurement Protocol API secrets
    /// </summary>
    public string ApiSecret { get; set; } = string.Empty;

    /// <summary>
    /// Google Ads Customer ID for Enhanced Conversions (optional)
    /// </summary>
    public string? GoogleAdsCustomerId { get; set; }

    /// <summary>
    /// Google Ads Conversion Action ID (optional)
    /// </summary>
    public string? ConversionActionId { get; set; }

    /// <summary>
    /// Enable debug mode to validate events
    /// </summary>
    public bool DebugMode { get; set; } = false;

    public bool IsEnabled => !string.IsNullOrEmpty(MeasurementId) && !string.IsNullOrEmpty(ApiSecret);
}
