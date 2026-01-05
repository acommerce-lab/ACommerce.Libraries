namespace ACommerce.Marketing.SnapchatConversions;

/// <summary>
/// Configuration options for Snapchat Conversions API
/// </summary>
public class SnapchatConversionsOptions
{
    public const string SectionName = "SnapchatConversions";

    /// <summary>
    /// Snapchat Pixel ID
    /// </summary>
    public string PixelId { get; set; } = string.Empty;

    /// <summary>
    /// Snapchat Access Token (long-lived token from Business Manager)
    /// </summary>
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>
    /// Test mode for debugging
    /// </summary>
    public bool TestMode { get; set; } = false;

    public bool IsEnabled => !string.IsNullOrEmpty(PixelId) && !string.IsNullOrEmpty(AccessToken);
}
