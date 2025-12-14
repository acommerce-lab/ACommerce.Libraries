namespace ACommerce.Templates.Customer.Services;

/// <summary>
/// Payment Service - Handles payment page opening
/// </summary>
public interface IPaymentService
{
    /// <summary>
    /// Open payment page in external browser
    /// </summary>
    Task<bool> OpenPaymentPageAsync(string paymentUrl);

    /// <summary>
    /// Opens payment page in an in-app WebView and waits for result
    /// </summary>
    /// <param name="paymentUrl">Payment gateway URL</param>
    /// <param name="callbackPattern">URL pattern to detect payment completion</param>
    /// <returns>Payment result with success/failure and transaction details</returns>
    Task<PaymentResult> OpenPaymentInAppAsync(string paymentUrl, string callbackPattern = "/host/payment/callback");

    /// <summary>
    /// Can open external links?
    /// </summary>
    bool CanOpenExternal { get; }

    /// <summary>
    /// Does the app support in-app WebView for payment?
    /// </summary>
    bool SupportsInAppPayment { get; }
}

/// <summary>
/// Payment result from in-app payment
/// </summary>
public class PaymentResult
{
    public bool Success { get; set; }
    public bool Cancelled { get; set; }
    public string? OrderId { get; set; }
    public string? TransactionId { get; set; }
    public string? Status { get; set; }
    public string? Message { get; set; }
    public string? ErrorMessage { get; set; }
    public Dictionary<string, string>? AdditionalData { get; set; }
}
