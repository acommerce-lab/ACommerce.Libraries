using Microsoft.AspNetCore.Components.WebView;

namespace Ashare.App;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Handle URL loading to keep iframe content inside the app on iOS
    /// By default, iOS WKWebView opens external URLs in Safari
    /// </summary>
    private void OnUrlLoading(object? sender, UrlLoadingEventArgs e)
    {
        // Log for debugging
        Console.WriteLine($"[MainPage] UrlLoading: {e.Url}");

        // Get the URL being loaded
        var url = e.Url?.ToString() ?? string.Empty;

        // Allow all URLs to load inside the WebView instead of opening in external browser
        // This is important for legal pages displayed in iframes
        if (!string.IsNullOrEmpty(url))
        {
            // For http/https URLs that are external, we want to keep them in the WebView
            // especially for legal pages and other content displayed in iframes
            if (url.StartsWith("http://") || url.StartsWith("https://"))
            {
                Console.WriteLine($"[MainPage] Loading URL in WebView: {url}");
                e.UrlLoadingStrategy = UrlLoadingStrategy.OpenInWebView;
                return;
            }
        }

        // For other URL schemes (like tel:, mailto:, etc.), use default behavior
        Console.WriteLine($"[MainPage] Using default strategy for: {url}");
    }
}
