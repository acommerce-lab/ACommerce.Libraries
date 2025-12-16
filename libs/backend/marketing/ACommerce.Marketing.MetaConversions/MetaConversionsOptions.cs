namespace ACommerce.Marketing.MetaConversions;

public class MetaConversionsOptions
{
    public const string SectionName = "MetaConversions";

    public string PixelId { get; set; } = string.Empty;
    
    public string AccessToken { get; set; } = string.Empty;

    public string ApiVersion { get; set; } = "v21.0";

    public string? TestEventCode { get; set; }

    public bool IsEnabled => !string.IsNullOrEmpty(PixelId) && !string.IsNullOrEmpty(AccessToken);
}
