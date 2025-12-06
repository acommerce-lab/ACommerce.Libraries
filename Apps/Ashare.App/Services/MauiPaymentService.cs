using Ashare.Shared.Services;
using Microsoft.Maui.ApplicationModel;

namespace Ashare.App.Services;

/// <summary>
/// MAUI implementation of payment service
/// Uses Browser.OpenAsync for external payment page
/// </summary>
public class MauiPaymentService : IPaymentService
{
    public bool CanOpenExternal => true;

    public async Task<bool> OpenPaymentPageAsync(string paymentUrl)
    {
        if (string.IsNullOrEmpty(paymentUrl))
        {
            Console.WriteLine("[MauiPaymentService] Payment URL is null or empty");
            return false;
        }

        try
        {
            Console.WriteLine($"[MauiPaymentService] Opening payment URL: {paymentUrl}");

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
