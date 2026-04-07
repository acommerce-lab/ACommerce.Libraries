using ACommerce.Payments.Abstractions.Enums;
using System.Web;

namespace Ashare.App.Pages;

/// <summary>
/// Payment WebView Page
/// Opens payment gateway URL and monitors for callback URL
/// </summary>
public partial class PaymentPage : ContentPage
{
    private readonly string _paymentUrl;
    private readonly string _callbackPattern;
    private readonly TaskCompletionSource<PaymentPageResult> _resultTcs;
    private bool _isCompleted = false;

    public Task<PaymentPageResult> ResultTask => _resultTcs.Task;

    public PaymentPage(string paymentUrl, string callbackPattern = "/host/payment/callback")
    {
        InitializeComponent();

        _paymentUrl = paymentUrl;
        _callbackPattern = callbackPattern;
        _resultTcs = new TaskCompletionSource<PaymentPageResult>();

        Console.WriteLine($"[PaymentPage] Initialized with URL: {paymentUrl}");
        Console.WriteLine($"[PaymentPage] Callback pattern: {callbackPattern}");

        // Load the payment URL
        PaymentWebView.Source = new UrlWebViewSource { Url = paymentUrl };
    }

    private void OnWebViewNavigating(object? sender, WebNavigatingEventArgs e)
    {
        Console.WriteLine($"[PaymentPage] Navigating to: {e.Url}");
        StatusLabel.Text = "جاري التحميل...";

        // Check if this is a callback URL
        if (!string.IsNullOrEmpty(e.Url) && e.Url.Contains(_callbackPattern))
        {
            Console.WriteLine($"[PaymentPage] ✅ Callback URL detected!");
            e.Cancel = true; // Don't navigate to callback URL
            ProcessCallbackUrl(e.Url);
        }
    }

    private void OnWebViewNavigated(object? sender, WebNavigatedEventArgs e)
    {
        Console.WriteLine($"[PaymentPage] Navigated: {e.Result}, URL: {e.Url}");

        // Hide loading overlay
        LoadingOverlay.IsVisible = false;
        StatusLabel.Text = "يتم معالجة الدفع عبر بوابة نون الآمنة";

        if (e.Result == WebNavigationResult.Failure)
        {
            Console.WriteLine($"[PaymentPage] ❌ Navigation failed");
            StatusLabel.Text = "فشل تحميل صفحة الدفع";
        }

        // Check if navigated to callback URL
        if (!string.IsNullOrEmpty(e.Url) && e.Url.Contains(_callbackPattern))
        {
            Console.WriteLine($"[PaymentPage] ✅ Callback URL detected in Navigated!");
            ProcessCallbackUrl(e.Url);
        }
    }

    private void ProcessCallbackUrl(string url)
    {
        if (_isCompleted) return;
        _isCompleted = true;

        Console.WriteLine($"[PaymentPage] Processing callback URL: {url}");

        try
        {
            var uri = new Uri(url);
            var query = HttpUtility.ParseQueryString(uri.Query);

            var status = query["status"] ?? query["Status"];
            var orderId = query["orderId"] ?? query["OrderId"];
            var transactionId = query["transactionId"] ?? query["TransactionId"];
            var merchantReference = query["merchantReference"] ?? query["MerchantReference"];
            var error = query["error"] ?? query["Error"];

            Console.WriteLine($"[PaymentPage] Callback params: orderId={orderId}, merchantReference={merchantReference}, status={status}, error={error}");

            // إذا كان هناك خطأ صريح من Noon، نعتبره فشل
            if (!string.IsNullOrEmpty(error))
            {
                Console.WriteLine($"[PaymentPage] ❌ Payment failed with error: {error}");
                _resultTcs.TrySetResult(new PaymentPageResult
                {
                    Success = false,
                    OrderId = orderId,
                    Status = PaymentStatus.Failed,
                    Message = error
                });
                ClosePageAsync();
                return;
            }

            // Noon لا يرسل status في redirect URL
            // يجب التحقق من حالة الدفع عبر API
            // نُرسل orderId للتحقق من الحالة الفعلية
            Console.WriteLine($"[PaymentPage] 🔄 Callback received - returning orderId for API verification");
            Console.WriteLine($"[PaymentPage] OrderId={orderId}, Status=pending_verification");

            var result = new PaymentPageResult
            {
                Success = false, // لا نفترض النجاح - يجب التحقق عبر API
                OrderId = orderId ?? merchantReference, // استخدام merchantReference كبديل
                TransactionId = transactionId ?? orderId,
                Status = PaymentStatus.Pending, // حالة تحتاج تحقق من API
                Message = "جاري التحقق من حالة الدفع..."
            };

            // Complete and close
            _resultTcs.TrySetResult(result);
            ClosePageAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[PaymentPage] Error processing callback: {ex.Message}");
            _resultTcs.TrySetResult(new PaymentPageResult
            {
                Success = false,
                Status = PaymentStatus.Failed,
                Message = "حدث خطأ في معالجة نتيجة الدفع"
            });
            ClosePageAsync();
        }
    }

    private async void OnCloseClicked(object? sender, EventArgs e)
    {
        Console.WriteLine("[PaymentPage] User cancelled payment");

        if (!_isCompleted)
        {
            _isCompleted = true;
            _resultTcs.TrySetResult(new PaymentPageResult
            {
                Success = false,
                Cancelled = true,
                Message = "تم إلغاء عملية الدفع"
            });
        }

        await ClosePageAsync();
    }

    private async Task ClosePageAsync()
    {
        try
        {
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                if (Navigation.ModalStack.Count > 0)
                {
                    await Navigation.PopModalAsync();
                }
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[PaymentPage] Error closing page: {ex.Message}");
        }
    }

    protected override bool OnBackButtonPressed()
    {
        // Handle Android back button
        if (!_isCompleted)
        {
            _isCompleted = true;
            _resultTcs.TrySetResult(new PaymentPageResult
            {
                Success = false,
                Cancelled = true,
                Message = "تم إلغاء عملية الدفع"
            });
        }

        ClosePageAsync();
        return true; // Prevent default back behavior
    }
}

/// <summary>
/// Result from the payment page
/// </summary>
public class PaymentPageResult
{
    public bool Success { get; set; }
    public bool Cancelled { get; set; }
    public string? OrderId { get; set; }
    public string? TransactionId { get; set; }
    public PaymentStatus? Status { get; set; }
    public string? Message { get; set; }
}
