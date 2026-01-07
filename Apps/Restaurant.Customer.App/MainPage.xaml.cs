using Microsoft.AspNetCore.Components.WebView;

namespace Restaurant.Customer.App;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
    }

    private void OnUrlLoading(object? sender, UrlLoadingEventArgs e)
    {
        var url = e.Url?.ToString() ?? string.Empty;

        if (!string.IsNullOrEmpty(url))
        {
            if (url.StartsWith("http://") || url.StartsWith("https://"))
            {
                e.UrlLoadingStrategy = UrlLoadingStrategy.OpenInWebView;
                return;
            }
        }
    }
}
