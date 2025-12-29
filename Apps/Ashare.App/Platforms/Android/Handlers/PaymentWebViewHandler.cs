using Android.Webkit;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using AWebView = Android.Webkit.WebView;

namespace Ashare.App.Platforms.Android.Handlers;

/// <summary>
/// Custom WebView handler for Android that enables 3DS/OTP payment verification
/// Handles JavaScript, popups, and mixed content required by bank verification pages
/// </summary>
public class PaymentWebViewHandler : WebViewHandler
{
    protected override void ConnectHandler(AWebView platformView)
    {
        base.ConnectHandler(platformView);

        // Enable JavaScript (required for 3DS)
        platformView.Settings.JavaScriptEnabled = true;

        // Enable DOM storage (required for some payment gateways)
        platformView.Settings.DomStorageEnabled = true;

        // Enable database storage
        platformView.Settings.DatabaseEnabled = true;

        // Allow mixed content (HTTP on HTTPS pages - some 3DS pages need this)
        platformView.Settings.MixedContentMode = MixedContentHandling.AlwaysAllow;

        // Enable JavaScript to open windows (for popup-based 3DS)
        platformView.Settings.JavaScriptCanOpenWindowsAutomatically = true;
        platformView.Settings.SetSupportMultipleWindows(true);

        // Enable zoom controls
        platformView.Settings.BuiltInZoomControls = true;
        platformView.Settings.DisplayZoomControls = false;

        // Set user agent to avoid being blocked by some payment gateways
        var defaultUserAgent = platformView.Settings.UserAgentString;
        platformView.Settings.UserAgentString = defaultUserAgent + " AshareApp/1.0";

        // Enable file access
        platformView.Settings.AllowFileAccess = true;
        platformView.Settings.AllowContentAccess = true;

        // Set custom WebChromeClient to handle popups and JavaScript dialogs
        platformView.SetWebChromeClient(new PaymentWebChromeClient(platformView));

        Console.WriteLine("[PaymentWebView] Custom handler configured for 3DS support");
    }
}

/// <summary>
/// Custom WebChromeClient that handles popups and JavaScript dialogs for 3DS
/// </summary>
public class PaymentWebChromeClient : WebChromeClient
{
    private readonly AWebView _parentWebView;

    public PaymentWebChromeClient(AWebView parentWebView)
    {
        _parentWebView = parentWebView;
    }

    /// <summary>
    /// Handle new window requests (popup-based 3DS verification)
    /// </summary>
    public override bool OnCreateWindow(AWebView? view, bool isDialog, bool isUserGesture, Android.OS.Message? resultMsg)
    {
        Console.WriteLine($"[PaymentWebView] OnCreateWindow: isDialog={isDialog}, isUserGesture={isUserGesture}");

        if (view == null || resultMsg == null)
            return false;

        // Get the hit test result to get the URL
        var hitTestResult = view.HitTestResult;
        var url = hitTestResult?.Extra;

        Console.WriteLine($"[PaymentWebView] New window URL: {url}");

        // Create a new WebView for the popup
        var newWebView = new AWebView(view.Context!);
        newWebView.Settings.JavaScriptEnabled = true;
        newWebView.Settings.DomStorageEnabled = true;
        newWebView.Settings.MixedContentMode = MixedContentHandling.AlwaysAllow;

        // Set up the transport
        var transport = (WebView.WebViewTransport?)resultMsg.Obj;
        if (transport != null)
        {
            transport.WebView = newWebView;
            resultMsg.SendToTarget();

            // Load the popup in the parent WebView instead of a new window
            // This keeps everything in the same context
            newWebView.SetWebViewClient(new PopupWebViewClient(_parentWebView));
        }

        return true;
    }

    /// <summary>
    /// Handle JavaScript alerts
    /// </summary>
    public override bool OnJsAlert(AWebView? view, string? url, string? message, JsResult? result)
    {
        Console.WriteLine($"[PaymentWebView] JS Alert: {message}");
        result?.Confirm();
        return true;
    }

    /// <summary>
    /// Handle JavaScript confirms
    /// </summary>
    public override bool OnJsConfirm(AWebView? view, string? url, string? message, JsResult? result)
    {
        Console.WriteLine($"[PaymentWebView] JS Confirm: {message}");
        result?.Confirm();
        return true;
    }

    /// <summary>
    /// Handle JavaScript prompts
    /// </summary>
    public override bool OnJsPrompt(AWebView? view, string? url, string? message, string? defaultValue, JsPromptResult? result)
    {
        Console.WriteLine($"[PaymentWebView] JS Prompt: {message}");
        result?.Confirm(defaultValue);
        return true;
    }
}

/// <summary>
/// WebViewClient for popup windows that redirects navigation back to parent
/// </summary>
public class PopupWebViewClient : WebViewClient
{
    private readonly AWebView _parentWebView;

    public PopupWebViewClient(AWebView parentWebView)
    {
        _parentWebView = parentWebView;
    }

    public override bool ShouldOverrideUrlLoading(AWebView? view, IWebResourceRequest? request)
    {
        var url = request?.Url?.ToString();
        Console.WriteLine($"[PaymentWebView] Popup navigation: {url}");

        if (!string.IsNullOrEmpty(url))
        {
            // Load in parent WebView
            _parentWebView.LoadUrl(url);
        }

        return true;
    }
}
