namespace ACommerce.Notifications.Channels.Sms.Options;

/// <summary>
/// إعدادات قناة SMS.
/// تدعم أي مزود SMS عبر HTTP REST.
/// </summary>
public class SmsOptions
{
    public const string SectionName = "Notifications:Sms";

    /// <summary>رقم المُرسل أو اسمه (Sender ID)</summary>
    public required string SenderNumber { get; set; }

    /// <summary>اسم المُرسل الظاهر للمستلم (اختياري)</summary>
    public string? SenderName { get; set; }

    /// <summary>
    /// نوع المزود. مدعوم: "Http" (افتراضي), "Twilio", "Unifonic"
    /// </summary>
    public string Provider { get; set; } = "Http";

    /// <summary>إعدادات المزود العام عبر HTTP</summary>
    public HttpSmsGatewayOptions Http { get; set; } = new();

    /// <summary>إعدادات Twilio (إذا كان Provider = "Twilio")</summary>
    public TwilioSmsOptions? Twilio { get; set; }
}

public class HttpSmsGatewayOptions
{
    /// <summary>رابط API المزود</summary>
    public string BaseUrl { get; set; } = string.Empty;

    /// <summary>مفتاح API</summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>اسم حقل الرقم في body الطلب</summary>
    public string PhoneFieldName { get; set; } = "to";

    /// <summary>اسم حقل الرسالة في body الطلب</summary>
    public string MessageFieldName { get; set; } = "message";

    /// <summary>اسم حقل المُرسل في body الطلب</summary>
    public string SenderFieldName { get; set; } = "from";

    /// <summary>headers إضافية</summary>
    public Dictionary<string, string> Headers { get; set; } = new();

    /// <summary>مهلة الطلب</summary>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);
}

public class TwilioSmsOptions
{
    public string AccountSid { get; set; } = string.Empty;
    public string AuthToken { get; set; } = string.Empty;
}
