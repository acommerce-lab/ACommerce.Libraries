using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using ACommerce.Templates.Customer.Services;

namespace Ashare.Shared.Services;

/// <summary>
/// تنفيذ خدمة التنقل مع دعم السجل (للويب)
/// </summary>
public class AppNavigationService(
    NavigationManager navigationManager,
    IJSRuntime jsRuntime)
    : IAppNavigationService
{
    private readonly Stack<string> _history = new();

    public string CurrentUrl => navigationManager.Uri;
    public string CurrentUri => navigationManager.Uri;

    public void NavigateTo(string url, bool forceLoad)
    {
        _history.Push(navigationManager.Uri);
        navigationManager.NavigateTo(url);
    }

    public void NavigateBack()
    {
        if (_history.Count > 0)
        {
            var previousUrl = _history.Pop();
            navigationManager.NavigateTo(previousUrl);
        }
        else
        {
            navigationManager.NavigateTo("/");
        }
    }

    public void NavigateToAndClearHistory(string uri)
    {
        // Clear navigation history
        _history.Clear();
        // Navigate with force reload
        navigationManager.NavigateTo(uri, forceLoad: true);
    }

    /// <summary>
    /// Open location in maps (web version - uses JavaScript)
    /// </summary>
    public async Task OpenMapAsync(double latitude, double longitude, string? label = null)
    {
        try
        {
            // Use the JavaScript function
            await jsRuntime.InvokeVoidAsync("AcMap.openNativeMaps", latitude, longitude, label ?? "الموقع");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[AppNavigationService] Error opening map: {ex.Message}");

            // Fallback: open Google Maps in new tab
            var url = $"https://www.google.com/maps?q={latitude},{longitude}";
            await jsRuntime.InvokeVoidAsync("window.open", url, "_blank");
        }
    }

    /// <summary>
    /// Open external URL (web version)
    /// </summary>
    public async Task OpenExternalAsync(string url)
    {
        try
        {
            if (string.IsNullOrEmpty(url))
            {
                Console.WriteLine("[AppNavigationService] OpenExternalAsync: URL is null or empty");
                return;
            }

            await jsRuntime.InvokeVoidAsync("window.open", url, "_blank");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[AppNavigationService] Error opening external URL: {ex.Message}");
        }
    }
}
