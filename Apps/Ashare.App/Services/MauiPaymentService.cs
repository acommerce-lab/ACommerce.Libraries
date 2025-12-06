using Ashare.Shared.Services;
using Ashare.Shared.Models;
using Ashare.App.Pages;
using Microsoft.Maui.ApplicationModel;

namespace Ashare.App.Services;

/// <summary>
/// MAUI implementation of payment service
/// Supports both external browser and in-app WebView payment
/// </summary>
public class MauiPaymentService : IPaymentService
{
    public bool CanOpenExternal => true;
    public bool SupportsInAppPayment => true;

    /// <summary>
    /// Open payment page in an in-app WebView
    /// This allows us to intercept the callback URL and get the payment result
    /// </summary>
    public async Task<PaymentResult> OpenPaymentInAppAsync(string paymentUrl, string callbackPattern = "/host/payment/callback")
    {
        if (string.IsNullOrEmpty(paymentUrl))
        {
            Console.WriteLine("[MauiPaymentService] Payment URL is null or empty");
            return new PaymentResult
            {
                Success = false,
                Message = "رابط الدفع غير متوفر"
            };
        }

        try
        {
            Console.WriteLine($"[MauiPaymentService] Opening payment in WebView: {paymentUrl}");
            Console.WriteLine($"[MauiPaymentService] Callback pattern: {callbackPattern}");

            // Create payment page
            var paymentPage = new PaymentPage(paymentUrl, callbackPattern);

            // Get the current page to navigate from
            var currentPage = Application.Current?.Windows?.FirstOrDefault()?.Page;
            if (currentPage == null)
            {
                Console.WriteLine("[MauiPaymentService] Cannot get current page");
                return new PaymentResult
                {
                    Success = false,
                    Message = "لا يمكن فتح صفحة الدفع"
                };
            }

            // Push payment page as modal
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                await currentPage.Navigation.PushModalAsync(paymentPage);
            });

            Console.WriteLine("[MauiPaymentService] Payment page opened, waiting for result...");

            // Wait for the payment result
            var pageResult = await paymentPage.ResultTask;

            Console.WriteLine($"[MauiPaymentService] Payment result: Success={pageResult.Success}, OrderId={pageResult.OrderId}");

            // Convert to shared PaymentResult
            return new PaymentResult
            {
                Success = pageResult.Success,
                Cancelled = pageResult.Cancelled,
                OrderId = pageResult.OrderId,
                TransactionId = pageResult.TransactionId ?? string.Empty,
                Status = pageResult.Status,
                Message = pageResult.Message
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[MauiPaymentService] Error opening payment WebView: {ex.Message}");
            return new PaymentResult
            {
                Success = false,
                Message = $"حدث خطأ: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Open payment page in external browser (fallback)
    /// </summary>
    public async Task<bool> OpenPaymentPageAsync(string paymentUrl)
    {
        if (string.IsNullOrEmpty(paymentUrl))
        {
            Console.WriteLine("[MauiPaymentService] Payment URL is null or empty");
            return false;
        }

        try
        {
            Console.WriteLine($"[MauiPaymentService] Opening payment URL in external browser: {paymentUrl}");

            // Method 1: Browser.OpenAsync with External mode (opens default browser)
            var result = await Browser.Default.OpenAsync(paymentUrl, BrowserLaunchMode.External);

            if (result)
            {
                Console.WriteLine("[MauiPaymentService] Successfully opened in external browser");
                return true;
            }

            Console.WriteLine("[MauiPaymentService] Browser.OpenAsync returned false, trying SystemPreferred");

            // Method 2: Try SystemPreferred (in-app browser)
            result = await Browser.Default.OpenAsync(paymentUrl, BrowserLaunchMode.SystemPreferred);

            if (result)
            {
                Console.WriteLine("[MauiPaymentService] Successfully opened with SystemPreferred");
                return true;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[MauiPaymentService] Browser.OpenAsync failed: {ex.Message}");
        }

        // Method 3: Fallback to Launcher
        try
        {
            Console.WriteLine("[MauiPaymentService] Trying Launcher fallback");
            var launched = await Launcher.Default.OpenAsync(new Uri(paymentUrl));

            if (launched)
            {
                Console.WriteLine("[MauiPaymentService] Launcher succeeded");
                return true;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[MauiPaymentService] Launcher also failed: {ex.Message}");
        }

        Console.WriteLine("[MauiPaymentService] All methods failed");
        return false;
    }
}
