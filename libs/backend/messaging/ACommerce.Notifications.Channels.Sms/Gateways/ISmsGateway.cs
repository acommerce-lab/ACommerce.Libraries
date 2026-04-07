namespace ACommerce.Notifications.Channels.Sms.Gateways;

/// <summary>
/// واجهة بوابة SMS.
/// كل مزود (Twilio, Unifonic, Jawal, etc.) يطبق هذه الواجهة.
/// </summary>
public interface ISmsGateway
{
    /// <summary>
    /// إرسال رسالة SMS.
    /// </summary>
    Task<SmsResult> SendAsync(
        string toNumber,
        string message,
        string? from = null,
        CancellationToken ct = default);
}

/// <summary>نتيجة إرسال SMS</summary>
public record SmsResult(
    bool Success,
    string? MessageId = null,
    string? Error = null,
    string? RawResponse = null);
