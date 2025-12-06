using Ashare.Shared.Models;

namespace Ashare.Shared.Services;

/// <summary>
/// خدمة الدفع
/// Payment Service - Handles payment page opening
/// </summary>
public interface IPaymentService
{
    /// <summary>
    /// فتح صفحة الدفع في المتصفح الخارجي
    /// </summary>
    Task<bool> OpenPaymentPageAsync(string paymentUrl);

    /// <summary>
    /// فتح صفحة الدفع في WebView داخل التطبيق
    /// Opens payment page in an in-app WebView and waits for result
    /// </summary>
    /// <param name="paymentUrl">Payment gateway URL</param>
    /// <param name="callbackPattern">URL pattern to detect payment completion (e.g. "/host/payment/callback")</param>
    /// <returns>Payment result with success/failure and transaction details</returns>
    Task<PaymentResult> OpenPaymentInAppAsync(string paymentUrl, string callbackPattern = "/host/payment/callback");

    /// <summary>
    /// هل يمكن فتح روابط خارجية؟
    /// </summary>
    bool CanOpenExternal { get; }

    /// <summary>
    /// هل يدعم التطبيق WebView داخلي للدفع؟
    /// </summary>
    bool SupportsInAppPayment { get; }
}
