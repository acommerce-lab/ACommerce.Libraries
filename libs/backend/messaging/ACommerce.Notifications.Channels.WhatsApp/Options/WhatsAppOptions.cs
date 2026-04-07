namespace ACommerce.Notifications.Channels.WhatsApp.Options;

/// <summary>
/// إعدادات قناة WhatsApp Business.
/// </summary>
public class WhatsAppOptions
{
    public const string SectionName = "Notifications:WhatsApp";

    /// <summary>رقم واتساب Business المُرسل (بصيغة دولية: +966...)</summary>
    public required string FromNumber { get; set; }

    /// <summary>
    /// نوع المزود: "CloudApi" (Meta), "Twilio", "Http" (مخصص)
    /// </summary>
    public string Provider { get; set; } = "CloudApi";

    /// <summary>إعدادات WhatsApp Cloud API (Meta)</summary>
    public CloudApiOptions CloudApi { get; set; } = new();

    /// <summary>إعدادات Twilio WhatsApp</summary>
    public TwilioWhatsAppOptions? Twilio { get; set; }

    /// <summary>إعدادات HTTP عامة</summary>
    public HttpWhatsAppGatewayOptions Http { get; set; } = new();
}

/// <summary>إعدادات Meta WhatsApp Cloud API</summary>
public class CloudApiOptions
{
    /// <summary>رمز الوصول الدائم من Meta Developer Portal</summary>
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>معرف رقم الهاتف في Meta</summary>
    public string PhoneNumberId { get; set; } = string.Empty;

    /// <summary>API URL (افتراضي: v19.0)</summary>
    public string ApiVersion { get; set; } = "v19.0";

    public string GetSendUrl() =>
        $"https://graph.facebook.com/{ApiVersion}/{PhoneNumberId}/messages";
}

public class TwilioWhatsAppOptions
{
    public string AccountSid { get; set; } = string.Empty;
    public string AuthToken { get; set; } = string.Empty;
}

public class HttpWhatsAppGatewayOptions
{
    public string BaseUrl { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public Dictionary<string, string> Headers { get; set; } = new();
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);
}
