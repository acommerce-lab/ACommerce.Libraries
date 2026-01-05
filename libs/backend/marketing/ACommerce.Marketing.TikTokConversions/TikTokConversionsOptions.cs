namespace ACommerce.Marketing.TikTokConversions;

/// <summary>
/// Configuration options for TikTok Events API
/// </summary>
public class TikTokConversionsOptions
{
    public const string SectionName = "TikTokConversions";

    /// <summary>
    /// TikTok Pixel ID
    /// </summary>
    public string PixelCode { get; set; } = string.Empty;

    /// <summary>
    /// TikTok Events API Access Token
    /// </summary>
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>
    /// Test Event Code for debugging (optional)
    /// </summary>
    public string? TestEventCode { get; set; }

    public bool IsEnabled => !string.IsNullOrEmpty(PixelCode) && !string.IsNullOrEmpty(AccessToken);
}
