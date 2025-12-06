namespace Ashare.Shared.Services;

/// <summary>
/// خدمة الدفع - تفتح صفحة الدفع الخارجية
/// Payment Service - Opens external payment page
/// </summary>
public interface IPaymentService
{
    /// <summary>
    /// فتح صفحة الدفع في المتصفح الخارجي
    /// </summary>
    Task<bool> OpenPaymentPageAsync(string paymentUrl);

    /// <summary>
    /// هل يمكن فتح روابط خارجية؟
    /// </summary>
    bool CanOpenExternal { get; }
}
