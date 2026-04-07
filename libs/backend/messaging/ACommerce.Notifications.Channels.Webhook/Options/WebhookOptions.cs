namespace ACommerce.Notifications.Channels.Webhook.Options;

/// <summary>
/// إعدادات قناة Webhook.
/// </summary>
public class WebhookOptions
{
    public const string SectionName = "Notifications:Webhook";

    /// <summary>
    /// URL الافتراضي لإرسال الـ webhook إليه.
    /// يمكن تجاوزه لكل إشعار عبر Notification.Data["WebhookUrl"].
    /// </summary>
    public string DefaultUrl { get; set; } = string.Empty;

    /// <summary>
    /// سر HMAC لتوقيع الطلبات (اختياري).
    /// إذا كان موجوداً، يُضاف header: X-ACommerce-Signature: sha256=...
    /// </summary>
    public string? HmacSecret { get; set; }

    /// <summary>اسم header التوقيع</summary>
    public string SignatureHeaderName { get; set; } = "X-ACommerce-Signature";

    /// <summary>مهلة الطلب</summary>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(15);

    /// <summary>headers إضافية تُضاف لكل طلب</summary>
    public Dictionary<string, string> Headers { get; set; } = new();

    /// <summary>
    /// تنسيق الـ payload المُرسل.
    /// "Full" = كل حقول الإشعار | "Minimal" = id + title + message فقط
    /// </summary>
    public WebhookPayloadFormat PayloadFormat { get; set; } = WebhookPayloadFormat.Full;
}

public enum WebhookPayloadFormat
{
    Full,
    Minimal
}
