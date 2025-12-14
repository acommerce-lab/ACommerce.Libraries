using ACommerce.Templates.Customer.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Ashare.Web.Services;

/// <summary>
/// Web implementation of payment service
/// Uses window.location.href for reliable redirect
/// </summary>
public class WebPaymentService : IPaymentService
{
    private readonly NavigationManager _navigation;
    private readonly IJSRuntime _js;

    public WebPaymentService(NavigationManager navigation, IJSRuntime js)
    {
        _navigation = navigation;
        _js = js;
    }

    public bool CanOpenExternal => true;

    /// <summary>
    /// Web doesn't support in-app WebView - uses redirect instead
    /// </summary>
    public bool SupportsInAppPayment => false;

    /// <summary>
    /// Not supported on web - use OpenPaymentPageAsync instead
    /// </summary>
    public Task<PaymentResult> OpenPaymentInAppAsync(string paymentUrl, string callbackPattern = "/host/payment/callback")
    {
        // Web uses redirect, not in-app WebView
        // This shouldn't be called since SupportsInAppPayment is false
        return Task.FromResult(new PaymentResult
        {
            Success = false,
            Message = "In-app payment not supported on web"
        });
    }

    public async Task<bool> OpenPaymentPageAsync(string paymentUrl)
    {
        if (string.IsNullOrEmpty(paymentUrl))
        {
            Console.WriteLine("[WebPaymentService] Payment URL is null or empty");
            return false;
        }

        try
        {
            Console.WriteLine($"[WebPaymentService] Opening payment URL: {paymentUrl}");

            // Method 1: Direct navigation (most reliable for same-tab redirect)
            _navigation.NavigateTo(paymentUrl, forceLoad: true);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[WebPaymentService] NavigateTo failed: {ex.Message}");

            try
            {
                // Method 2: window.location.href (fallback)
                await _js.InvokeVoidAsync("eval", $"window.location.href = '{paymentUrl}'");
                return true;
            }
            catch (Exception jsEx)
            {
                Console.WriteLine($"[WebPaymentService] JS fallback also failed: {jsEx.Message}");

                try
                {
                    // Method 3: Open in new tab
                    await _js.InvokeVoidAsync("window.open", paymentUrl, "_blank");
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }
    }
}
