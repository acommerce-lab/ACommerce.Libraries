using Microsoft.AspNetCore.Components;
using ACommerce.Templates.Customer.Services;

namespace Ashare.App.Services;

/// <summary>
/// MAUI implementation of navigation service
/// </summary>
public class AppNavigationService : IAppNavigationService
{
    private readonly NavigationManager _navigationManager;
    private readonly Stack<string> _history = new();

    public AppNavigationService(NavigationManager navigationManager)
    {
        _navigationManager = navigationManager;
    }

    public string CurrentUri => _navigationManager.Uri;

    public void NavigateTo(string uri, bool forceLoad = false)
    {
        _history.Push(_navigationManager.Uri);
        _navigationManager.NavigateTo(uri, forceLoad);
    }

    public void NavigateBack()
    {
        if (_history.Count > 0)
        {
            var previousUri = _history.Pop();
            _navigationManager.NavigateTo(previousUri);
        }
        else
        {
            _navigationManager.NavigateTo("/");
        }
    }

    public void NavigateToAndClearHistory(string uri)
    {
        // Clear all navigation history to prevent back button returning to authenticated pages
        _history.Clear();
        // Navigate with force reload to ensure fresh state
        _navigationManager.NavigateTo(uri, forceLoad: true);
    }

    /// <summary>
    /// Open location in native maps app using MAUI Essentials
    /// </summary>
    public async Task OpenMapAsync(double latitude, double longitude, string? label = null)
    {
        try
        {
            var location = new Location(latitude, longitude);
            var options = new MapLaunchOptions
            {
                Name = label ?? "الموقع",
                NavigationMode = NavigationMode.None
            };

            await Microsoft.Maui.ApplicationModel.Map.Default.OpenAsync(location, options);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[AppNavigationService] Error opening map: {ex.Message}");

            // Fallback: try opening via Launcher with geo URI
            try
            {
                var geoUri = $"geo:{latitude},{longitude}?q={latitude},{longitude}({Uri.EscapeDataString(label ?? "الموقع")})";
                await Launcher.Default.OpenAsync(new Uri(geoUri));
            }
            catch (Exception fallbackEx)
            {
                Console.WriteLine($"[AppNavigationService] Fallback also failed: {fallbackEx.Message}");

                // Last resort: open Google Maps web
                var webUrl = $"https://www.google.com/maps?q={latitude},{longitude}";
                await Launcher.Default.OpenAsync(new Uri(webUrl));
            }
        }
    }

    /// <summary>
    /// Open external URL using MAUI Browser (more reliable for web URLs)
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

            Console.WriteLine($"[AppNavigationService] Opening external URL: {url}");

            // Use Browser.OpenAsync for web URLs - more reliable than Launcher
            var result = await Browser.Default.OpenAsync(url, BrowserLaunchMode.External);

            if (!result)
            {
                Console.WriteLine("[AppNavigationService] Browser.OpenAsync returned false, trying Launcher fallback");
                // Fallback to Launcher if Browser fails
                await Launcher.Default.OpenAsync(new Uri(url));
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[AppNavigationService] Error opening external URL: {ex.Message}");

            // Last resort: try with Launcher
            try
            {
                await Launcher.Default.OpenAsync(new Uri(url));
            }
            catch (Exception fallbackEx)
            {
                Console.WriteLine($"[AppNavigationService] Fallback also failed: {fallbackEx.Message}");
            }
        }
    }
}
