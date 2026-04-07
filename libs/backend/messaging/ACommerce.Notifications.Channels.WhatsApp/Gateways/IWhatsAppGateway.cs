namespace ACommerce.Notifications.Channels.WhatsApp.Gateways;

/// <summary>
/// واجهة بوابة WhatsApp Business.
/// كل مزود (Meta Cloud API, Twilio, etc.) يطبق هذه الواجهة.
/// </summary>
public interface IWhatsAppGateway
{
    /// <summary>
    /// إرسال رسالة نصية WhatsApp.
    /// </summary>
    Task<WhatsAppResult> SendTextAsync(
        string toNumber,
        string message,
        CancellationToken ct = default);

    /// <summary>
    /// إرسال رسالة بناءً على template معتمد من Meta.
    /// </summary>
    Task<WhatsAppResult> SendTemplateAsync(
        string toNumber,
        string templateName,
        string languageCode,
        IEnumerable<string>? parameters = null,
        CancellationToken ct = default);
}

/// <summary>نتيجة إرسال رسالة WhatsApp</summary>
public record WhatsAppResult(
    bool Success,
    string? MessageId = null,
    string? Error = null,
    string? RawResponse = null);
