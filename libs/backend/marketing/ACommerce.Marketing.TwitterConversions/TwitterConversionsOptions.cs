namespace ACommerce.Marketing.TwitterConversions;

/// <summary>
/// Configuration options for Twitter/X Conversions API
/// </summary>
public class TwitterConversionsOptions
{
    public const string SectionName = "TwitterConversions";

    /// <summary>
    /// Twitter Pixel ID
    /// </summary>
    public string PixelId { get; set; } = string.Empty;

    /// <summary>
    /// Twitter Ads API Access Token
    /// </summary>
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>
    /// Twitter Ads Account ID
    /// </summary>
    public string? AccountId { get; set; }

    public bool IsEnabled => !string.IsNullOrEmpty(PixelId) && !string.IsNullOrEmpty(AccessToken);
}
